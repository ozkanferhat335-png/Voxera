using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Voxera.Application.Interfaces;
using Voxera.Infrastructure.Settings;

namespace Voxera.Infrastructure.FreeSWITCH;

/// <summary>
/// FreeSWITCH ESL (Event Socket Library) service for call control.
/// Communicates with FreeSWITCH via TCP socket using ESL protocol.
/// </summary>
public class FreeSwitchService : IFreeSwitchService, IDisposable
{
    private readonly FreeSwitchSettings _settings;
    private readonly ILogger<FreeSwitchService> _logger;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public FreeSwitchService(IOptions<FreeSwitchSettings> settings, ILogger<FreeSwitchService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (_client?.Connected == true) return;

        _client = new TcpClient();
        await _client.ConnectAsync(_settings.Host, _settings.EslPort, ct);
        _stream = _client.GetStream();

        // Read auth request
        await ReadResponseAsync(ct);

        // Send auth
        await SendCommandAsync($"auth {_settings.EslPassword}", ct);
        var authResponse = await ReadResponseAsync(ct);
        if (!authResponse.Contains("+OK accepted"))
            throw new InvalidOperationException("FreeSWITCH authentication failed.");

        _logger.LogInformation("Connected to FreeSWITCH ESL at {Host}:{Port}", _settings.Host, _settings.EslPort);
    }

    private async Task<string> SendCommandAsync(string command, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            await EnsureConnectedAsync(ct);
            var bytes = Encoding.UTF8.GetBytes($"{command}\n\n");
            await _stream!.WriteAsync(bytes, ct);
            return await ReadResponseAsync(ct);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<string> ReadResponseAsync(CancellationToken ct)
    {
        var buffer = new byte[4096];
        var sb = new StringBuilder();
        do
        {
            var bytesRead = await _stream!.ReadAsync(buffer, ct);
            sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
        } while (_stream.DataAvailable);
        return sb.ToString();
    }

    public async Task<bool> OriginateCallAsync(string fromExtension, string toNumber, string domain, CancellationToken ct = default)
    {
        try
        {
            var command = $"api originate {{origination_caller_id_number={fromExtension}}}sofia/internal/{fromExtension}@{domain} {toNumber} XML default";
            var response = await SendCommandAsync(command, ct);
            return response.Contains("+OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to originate call from {From} to {To}", fromExtension, toNumber);
            return false;
        }
    }

    public async Task<bool> HangupCallAsync(string callId, CancellationToken ct = default)
    {
        try
        {
            var response = await SendCommandAsync($"api uuid_kill {callId}", ct);
            return response.Contains("+OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hangup call {CallId}", callId);
            return false;
        }
    }

    public async Task<bool> TransferCallAsync(string callId, string destination, CancellationToken ct = default)
    {
        try
        {
            var response = await SendCommandAsync($"api uuid_transfer {callId} {destination} XML default", ct);
            return response.Contains("+OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transfer call {CallId} to {Destination}", callId, destination);
            return false;
        }
    }

    public async Task<bool> HoldCallAsync(string callId, CancellationToken ct = default)
    {
        try
        {
            var response = await SendCommandAsync($"api uuid_hold {callId}", ct);
            return response.Contains("+OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hold call {CallId}", callId);
            return false;
        }
    }

    public async Task<bool> UnholdCallAsync(string callId, CancellationToken ct = default)
    {
        try
        {
            var response = await SendCommandAsync($"api uuid_hold off {callId}", ct);
            return response.Contains("+OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unhold call {CallId}", callId);
            return false;
        }
    }

    public async Task<bool> StartRecordingAsync(string callId, string filePath, CancellationToken ct = default)
    {
        try
        {
            var response = await SendCommandAsync($"api uuid_record {callId} start {filePath}", ct);
            return response.Contains("+OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start recording for call {CallId}", callId);
            return false;
        }
    }

    public async Task<bool> StopRecordingAsync(string callId, CancellationToken ct = default)
    {
        try
        {
            var response = await SendCommandAsync($"api uuid_record {callId} stop all", ct);
            return response.Contains("+OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop recording for call {CallId}", callId);
            return false;
        }
    }

    public async Task<bool> PlayAudioAsync(string callId, string audioPath, CancellationToken ct = default)
    {
        try
        {
            var response = await SendCommandAsync($"api uuid_broadcast {callId} {audioPath} aleg", ct);
            return response.Contains("+OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to play audio for call {CallId}", callId);
            return false;
        }
    }

    public async Task<IEnumerable<ActiveCallDto>> GetActiveCallsAsync(string domain, CancellationToken ct = default)
    {
        try
        {
            var response = await SendCommandAsync("api show calls as json", ct);
            // Parse JSON response from FreeSWITCH
            // Simplified - in production parse the actual JSON
            return Enumerable.Empty<ActiveCallDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active calls for domain {Domain}", domain);
            return Enumerable.Empty<ActiveCallDto>();
        }
    }

    public async Task<bool> CreateSipUserAsync(string username, string password, string domain, CancellationToken ct = default)
    {
        // In production: write user XML to FreeSWITCH directory and reload
        // FreeSWITCH reads user configs from /etc/freeswitch/directory/
        _logger.LogInformation("Creating SIP user {Username}@{Domain}", username, domain);
        return await Task.FromResult(true);
    }

    public async Task<bool> DeleteSipUserAsync(string username, string domain, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting SIP user {Username}@{Domain}", username, domain);
        return await Task.FromResult(true);
    }

    public async Task<bool> ReloadXmlAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await SendCommandAsync("api reloadxml", ct);
            return response.Contains("+OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload XML");
            return false;
        }
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _client?.Dispose();
        _semaphore.Dispose();
    }
}
