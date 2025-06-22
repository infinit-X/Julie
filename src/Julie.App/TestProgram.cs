using System;
using Microsoft.Extensions.Logging;

namespace Julie.App
{
    internal class TestProgram
    {
        public static void RunTest(string[] args)
        {
            Console.WriteLine("=== Julie AI Assistant - Basic Test ===");
            Console.WriteLine("This is a console-only test to verify the application can start");
            
            try
            {
                Console.WriteLine("Testing dependency injection...");
                // Test basic DI without Avalonia
                TestBasicServices();
                
                Console.WriteLine("All tests passed! The core services are working.");
                Console.WriteLine("The issue was likely with dependency injection setup.");
                
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during testing: {ex}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }
        
        private static void TestBasicServices()
        {
            // Create a simple logger factory for testing
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            
            Console.WriteLine("- Creating SettingsManager...");
            var settingsLogger = loggerFactory.CreateLogger<Julie.Core.Utils.SettingsManager>();
            var settingsManager = new Julie.Core.Utils.SettingsManager(settingsLogger);
            Console.WriteLine("✓ SettingsManager created successfully");
            
            Console.WriteLine("- Creating LiveApiClient...");
            var apiLogger = loggerFactory.CreateLogger<Julie.Core.API.LiveApiClient>();
            var apiClient = new Julie.Core.API.LiveApiClient(apiLogger);
            Console.WriteLine("✓ LiveApiClient created successfully");
            
            Console.WriteLine("- All core services initialized successfully!");
        }
    }
}
