namespace Voxera.Infrastructure.Settings;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "voxera-api";
    public string Audience { get; set; } = "voxera-clients";
    public int ExpiryMinutes { get; set; } = 60;
}

public class FreeSwitchSettings
{
    public string Host { get; set; } = "localhost";
    public int EslPort { get; set; } = 8021;
    public string EslPassword { get; set; } = "ClueCon";
    public string ApiUrl { get; set; } = "http://localhost:8080";
    public string RecordingsPath { get; set; } = "/var/recordings";
}

public class RedisSettings
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public string InstanceName { get; set; } = "voxera:";
}

public class RabbitMqSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "voxera";
    public string Password { get; set; } = "voxera_pass";
    public string VirtualHost { get; set; } = "/";
}

public class AiSettings
{
    public string OpenAiApiKey { get; set; } = string.Empty;
    public string WhisperModel { get; set; } = "whisper-1";
    public string GptModel { get; set; } = "gpt-4o-mini";
}
