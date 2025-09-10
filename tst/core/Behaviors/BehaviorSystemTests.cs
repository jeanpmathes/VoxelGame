using System;
using JetBrains.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Tests.Utilities.Resources;
using Xunit;

namespace VoxelGame.Core.Tests.Behaviors;

[TestSubject(typeof(BehaviorSystem<,>))]
public class BehaviorSystemTests
{
    private class TestSubject : BehaviorContainer<TestSubject, TestBehaviorBase>
    {
        public Boolean Defined { get; private set; }
        public Boolean Subscribed { get; private set; }

        public override void DefineEvents(IEventRegistry registry)
        {
            Defined = true;
        }

        public override void SubscribeToEvents(IEventBus bus)
        {
            Subscribed = true;
        }
    }

    private abstract class TestBehaviorBase(TestSubject subject) : Behavior<TestBehaviorBase, TestSubject>(subject)
    {
        public Boolean Defined { get; private set; }
        public Boolean Subscribed { get; private set; }
        public Boolean Validated { get; private set; }

        public override void DefineEvents(IEventRegistry registry)
        {
            Defined = true;
        }

        public override void SubscribeToEvents(IEventBus bus)
        {
            Subscribed = true;
        }

        protected override void OnValidate(IValidator validator)
        {
            Validated = true;
        }
    }

    private sealed class BehaviorA(TestSubject subject) : TestBehaviorBase(subject),
        IBehavior<BehaviorA, TestBehaviorBase, TestSubject>
    {
        public static BehaviorA Construct(TestSubject subject) => new(subject);
    }

    private sealed class BehaviorB(TestSubject subject) : TestBehaviorBase(subject),
        IBehavior<BehaviorB, TestBehaviorBase, TestSubject>
    {
        public static BehaviorB Construct(TestSubject subject) => new(subject);
    }
    
    private sealed class BehaviorC(TestSubject subject) : TestBehaviorBase(subject),
        IBehavior<BehaviorC, TestBehaviorBase, TestSubject>
    {
        public static BehaviorC Construct(TestSubject subject) => new(subject);
    }
    
    [Fact]
    public void BehaviorSystem_Bake_ShouldBakeSubjectAndBehaviors()
    {
        TestSubject subject1 = new();
        var b1A = subject1.Require<BehaviorA>();
        var b1B = subject1.Require<BehaviorB>();
        TestSubject subject2 = new();
        var b2A = subject2.Require<BehaviorA>();
        var b2C = subject2.Require<BehaviorC>();

        Int32 count = BehaviorSystem<TestSubject, TestBehaviorBase>.Bake(new Validator(new MockResourceContext()));

        Assert.Equal(expected: 3, count);
        
        AssertCorrectSubject(subject1);
        AssertCorrectSubject(subject2);
        
        Assert.Same(b1A, subject1.Get<BehaviorA>());
        Assert.Same(b1B, subject1.Get<BehaviorB>());
        
        Assert.Same(b2A, subject2.Get<BehaviorA>());
        Assert.Same(b2C, subject2.Get<BehaviorC>());
    }

    private static void AssertCorrectSubject(TestSubject subject)
    {
        Assert.True(subject.Defined);
        Assert.True(subject.Subscribed);
        
        foreach (TestBehaviorBase behavior in subject.Behaviors)
        {
            Assert.True(behavior.Defined);
            Assert.True(behavior.Subscribed);
            Assert.True(behavior.Validated);
        }
    }
}
