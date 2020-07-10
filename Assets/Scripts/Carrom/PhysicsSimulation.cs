using System;
using UnityEngine;

namespace Carrom
{
    public abstract class PhysicsSimulation : SimulationMonoBase
    {
        public PhysicsScene scene;

        public virtual void Awake()
        {
            this.scene               = gameObject.scene.GetPhysicsScene();
        }

        public void SetManual(bool mode)
        {
            Physics.autoSimulation = !mode;
            Physics2D.autoSimulation = !mode;
        }

        public virtual void ApplyPendingForces()
        {
            scene.Simulate(Mathf.Epsilon);
        }

        public override void Simulate(float step, float duration)
        {
            while (duration >= step)
            {
                duration -= step;
                scene.Simulate(step);
            }
        }

        public override void Simulate(float step, Func<bool> exitCondition)
        {
            while (!exitCondition())
            {
                scene.Simulate(step);
            }
        }

        public override void Simulate(float step, Func<bool> exitCondition, int minimumSteps, int maximumSteps = Int32.MaxValue)
        {
            while ((!exitCondition() || minimumSteps > 0) && maximumSteps > 0)
            {
                minimumSteps--;
                maximumSteps--;
                scene.Simulate(step);
            }
        }

        public override void Simulate(float step, float maxDuration, Func<bool> exitCondition)
        {
            while (!exitCondition() && maxDuration >= 0.0f)
            {
                maxDuration -= step;
                scene.Simulate(step);
            }
        }
    }
}