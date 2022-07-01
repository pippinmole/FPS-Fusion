namespace FusionFps.Core {
    internal class SingletonProviderUsageExample {
        private interface IMessenger {
            void SendMessage();
        }

        public class Messenger : IMessenger {
            public static Messenger Instance { get; } = new();

            private Messenger() { }

            public void SendMessage() { }
        }

        void DemonstrateMethods() {
            // Register accessor method for singleton:
            ServiceProvider.AddSingleton<IMessenger>(() => Messenger.Instance);
            
            // Retrieve singleton instance for interface type:
            var messenger = ServiceProvider.Get<IMessenger>();
        }
    }
}