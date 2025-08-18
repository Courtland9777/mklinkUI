using System.Net;
using System.Net.Sockets;
using FluentAssertions;
using MklinkUi.WebUI;
using Xunit;

namespace MklinkUi.Tests;

public class PortSelectionTests
{
    [Fact]
    public void FindAvailablePort_skips_in_use_port()
    {
        var range = Program.ParseRange("3000-3005");
        using var listener = new TcpListener(IPAddress.Loopback, 3000);
        listener.Start();

        var port = Program.FindAvailablePort(3000, range);

        port.Should().BeGreaterThan(3000);
    }
}
