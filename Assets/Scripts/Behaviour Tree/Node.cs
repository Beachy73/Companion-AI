using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

/// <summary>
/// Execute can return one of three things
/// </summary>
public enum BTStatus
{
    RUNNING,
    SUCCESS,
    FAILURE
}

/// <summary>
/// Base class. Sets the foundation for the other nodes
/// </summary>
public abstract class BTNode
{
    protected Blackboard bb;
    public BTNode(Blackboard bb)
    {
        this.bb = bb;
    }

    public abstract BTStatus Execute();

    /// <summary>
    /// Reset should be overridden in child classes as and when necessary
    /// Should be called when a node is abruptly aborted before it can finish with success or failure
    /// i.e the node was still RUNNING when aborted
    /// </summary>
    public virtual void Reset()
    {
    }
}


public abstract class CompositeNode : BTNode
{
    protected int currentChildIndex = 0;
    protected List<BTNode> children;

    public CompositeNode(Blackboard bb) : base(bb)
    {
        children = new List<BTNode>();
    }

    public void AddChild(BTNode child)
    {
        children.Add(child);
    }

    /// <summary>
    /// When a composite node is reset it set the child index back to 0, it should propagate the reset down to all its children
    /// </summary>
    public override void Reset()
    {
        currentChildIndex = 0;

        // Reset every child
        for (int i = 0; i < children.Count; i++)
        {
            children[i].Reset();
        }
    }
}

/// <summary>
/// Selectors execute their children in order until a child succeeds, at which point it stops execution
/// If a child returns RUNNING, then it will need to stop execution but resume from the same point the next time it executes
/// </summary>
public class Selector : CompositeNode
{
    public Selector(Blackboard bb) : base(bb)
    {
    }

    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.FAILURE;

        // needs to reset after returning a success
        // for loop, if children is success, return success and reset
        // if running, should return running, doesn't reset, executes again from this point
        // once finished completely needs to reset

        for (int i = currentChildIndex; i < children.Count; i++)
        {
            if (children[i].Execute() == BTStatus.RUNNING)
            {
                rv = BTStatus.RUNNING;
                currentChildIndex = i;
                return rv;
            } 
            else if (children[i].Execute() == BTStatus.SUCCESS)
            {
                rv = BTStatus.SUCCESS;
                Reset();
                return rv;
            }
        }

        Reset();
        return rv;
    }
}

/// <summary>
/// Sequences execute their children in order until a child fails, at which point it stops execution
/// If a child returns RUNNING, then it will need to stop execution but resume from the same spot the next time it executes
/// </summary>
public class Sequence : CompositeNode
{
    public Sequence(Blackboard bb) : base(bb)
    {
    }
    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.SUCCESS;

        for (int i = currentChildIndex; i < children.Count; i++)
        {
            if (children[i].Execute() == BTStatus.RUNNING)
            {
                rv = BTStatus.RUNNING;
                currentChildIndex = i;
                return rv;
            }
            else if (children[i].Execute() == BTStatus.FAILURE)
            {
                rv = BTStatus.FAILURE;
                Reset();
                return rv;
            }
        }

        Reset();
        return rv;
    }
}

/// <summary>
/// Decorator nodes customise functionality of other nodes by wrapping around them
/// </summary>
public abstract class DecoratorNode : BTNode
{
    protected BTNode wrappedNode;
    public DecoratorNode(BTNode wrappedNode, Blackboard bb) : base(bb)
    {
        this.wrappedNode = wrappedNode;
    }

    public BTNode GetWrappedNode()
    {
        return wrappedNode;
    }

    public override void Reset()
    {
        wrappedNode.Reset();
    }
}

public class InverterDecorator : DecoratorNode
{
    public InverterDecorator(BTNode wrappedNode, Blackboard bb) : base(wrappedNode, bb)
    {

    }

    public override BTStatus Execute()
    {
        BTStatus rv = wrappedNode.Execute();

        if (rv == BTStatus.FAILURE)
        {
            rv = BTStatus.SUCCESS;
        } 
        else if (rv == BTStatus.SUCCESS)
        {
            rv = BTStatus.FAILURE;
        }

        return rv;
    }
}

/// <summary>
/// Inherit this and override CheckStatus. If that returns true 
/// </summary>
public abstract class ConditionalDecorator : DecoratorNode
{
    public ConditionalDecorator(BTNode wrappedNode, Blackboard bb) : base(wrappedNode, bb)
    { 
    }

    public abstract bool CheckStatus();
    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.FAILURE;

        if (CheckStatus())
        {
            rv = wrappedNode.Execute();
        }

        return rv;
    }
}

/// <summary>
/// This node simply returns success after the alloted delay time has passed
/// </summary>
public class DelayNode : BTNode
{
    protected float delay = 0.0f;
    bool started = false;
    private Timer regulator;
    bool delayFinished = false;

    public DelayNode(Blackboard bb, float delayTime) : base(bb)
    {
        this.delay = delayTime;
        regulator = new Timer(delay * 1000.0f); // in milliseconds, so multiply by 1000
        regulator.Elapsed += OnTimedEvent;
        regulator.Enabled = true;
        regulator.Stop();
    }

    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.RUNNING;

        if (!started && !delayFinished)
        {
            started = true;
            regulator.Start();
        }
        else if (delayFinished)
        {
            delayFinished = false;
            started = false;
            rv = BTStatus.SUCCESS;
        }

        return rv;
    }

    private void OnTimedEvent(object sender, ElapsedEventArgs e)
    {
        started = false;
        delayFinished = true;
        regulator.Stop();
    }

    // Timers count down independently of the Behaviour Tree, so we need to stop them when the behaviour is aborted/reset
    public override void Reset()
    {
        regulator.Stop();
        delayFinished = false;
        started = false;
    }
}
