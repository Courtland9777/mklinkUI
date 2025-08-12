#if WINDOWS
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Win32;
using Xunit;

namespace MklinlUi.Windows.Tests;

public class DeveloperModeServiceTests
{
    private const uint HKEY_LOCAL_MACHINE = 0x80000002;

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegOverridePredefKey(UIntPtr hKey, IntPtr hNewKey);

    [Fact]
    public async Task IsEnabledAsync_returns_false_on_non_Windows()
    {
        var service = new DeveloperModeService();
        if (OperatingSystem.IsWindows())
        {
            return; // environment cannot verify non-Windows path
        }

        var result = await service.IsEnabledAsync();
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEnabledAsync_returns_true_when_registry_value_is_nonzero()
    {
        using var root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
            .CreateSubKey("TestDevMode", writable: true);
        OverrideHKLM(root);
        try
        {
            using var sub = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock");
            sub.SetValue("AllowDevelopmentWithoutDevLicense", 1, RegistryValueKind.DWord);

            var service = new DeveloperModeService();
            var result = await service.IsEnabledAsync();

            result.Should().BeTrue();
        }
        finally
        {
            RestoreHKLM();
            Registry.CurrentUser.DeleteSubKeyTree("TestDevMode", throwOnMissingSubKey: false);
        }
    }

    [Fact]
    public async Task IsEnabledAsync_wraps_exceptions_from_registry_access()
    {
        using var root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
            .CreateSubKey("TestDevMode", writable: true);
        var handle = root.Handle.DangerousGetHandle();
        root.Dispose();
        OverrideHKLM(handle);
        try
        {
            var service = new DeveloperModeService();
            Func<Task> act = () => service.IsEnabledAsync();

            await act.Should().ThrowAsync<InvalidOperationException>();
        }
        finally
        {
            RestoreHKLM();
            Registry.CurrentUser.DeleteSubKeyTree("TestDevMode", throwOnMissingSubKey: false);
        }
    }

    private static void OverrideHKLM(RegistryKey key) =>
        RegOverridePredefKey((UIntPtr)HKEY_LOCAL_MACHINE, key.Handle.DangerousGetHandle());

    private static void OverrideHKLM(IntPtr handle) =>
        RegOverridePredefKey((UIntPtr)HKEY_LOCAL_MACHINE, handle);

    private static void RestoreHKLM() =>
        RegOverridePredefKey((UIntPtr)HKEY_LOCAL_MACHINE, IntPtr.Zero);
}
#endif
