using System.Collections.Generic;

namespace Leontitas
{
    public class Feature : ISystem, IInitializeSystem, IExecuteSystem, ICleanupSystem, ITearDownSystem
    {
        private List<IInitializeSystem> _initializeSystems = new List<IInitializeSystem>();
        private List<IExecuteSystem> _executeSystems = new List<IExecuteSystem>();
        private List<ICleanupSystem> _cleanupSystems = new List<ICleanupSystem>();
        private List<ITearDownSystem> _tearDownSystems = new List<ITearDownSystem>();
        
        public virtual void Add(ISystem system)
        {
            if (system is IInitializeSystem initializeSystem)
            {
                _initializeSystems.Add(initializeSystem);
            }
            if (system is IExecuteSystem executeSystem)
            {
                _executeSystems.Add(executeSystem);
            }
            if (system is ICleanupSystem cleanupSystem)
            {
                _cleanupSystems.Add(cleanupSystem);
            }
            if (system is ITearDownSystem tearDownSystem)
            {
                _tearDownSystems.Add(tearDownSystem);
            }
        }

        public void Initialize()
        {
            foreach (IInitializeSystem initializeSystem in _initializeSystems)
            {
                initializeSystem.Initialize();
            }
        }

        public void Execute()
        {
            foreach (IExecuteSystem executeSystem in _executeSystems)
            {
                executeSystem.Execute();
            }
        }

        public void Cleanup()
        {
            foreach (ICleanupSystem cleanupSystem in _cleanupSystems)
            {
                cleanupSystem.Cleanup();
            }
        }

        public void TearDown()
        {
            foreach (ITearDownSystem tearDownSystem in _tearDownSystems)
            {
                tearDownSystem.TearDown();
            }
        }
    }
}