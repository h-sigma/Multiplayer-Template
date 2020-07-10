using System;

namespace Carrom
{
    public interface ISimulation
    {
        void Simulate(float step, float duration);
        void Simulate(float step, Func<bool> exitCondition);
        void Simulate(float step, float maxDuration, Func<bool> exitCondition);

        void Generate();
    }
}