using Microsoft.AspNetCore.Mvc;

namespace PgpCore.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class PgpController : ControllerBase
{
    private readonly ILogger<PgpController> _logger;
    private readonly FileInfo _privateKey = new FileInfo("Keys/privateKey.asc");
    private readonly FileInfo _publicKey = new FileInfo("Keys/publicKey.asc");
    private readonly string _userName = "username";
    private readonly string _password = "password";

    public PgpController(ILogger<PgpController> logger)
    {
        _logger = logger;
    }

    [HttpPost("Encrypt")]
    public async Task<IActionResult> Encrypt(IFormFile file, CancellationToken cancellationToken)
    {
        EncryptionKeys encryptionKeys = new EncryptionKeys(
            _publicKey.OpenRead());

        PGP pgpEncrypt = new PGP(encryptionKeys);

        MemoryStream encryptedStream = new MemoryStream();

        await pgpEncrypt.EncryptStreamAsync(
            file.OpenReadStream(),
            encryptedStream
            );

        string filename = file.ContentDisposition.Split(';').FirstOrDefault(f => f.Contains("filename="))?.Split('=')[1].Trim('"') ?? file.Name;

        return new FileContentResult(encryptedStream.ToArray(), file.ContentType) { FileDownloadName = $"{filename}.pgp" };
    }

    [HttpPost("Decrypt")]
    public async Task<IActionResult> Decrypt(IFormFile file, CancellationToken cancellationToken)
    {
        EncryptionKeys encryptionKeys = new EncryptionKeys(
            _privateKey.OpenRead(),
            _password
            );

        PGP pgpDecrypt = new PGP(encryptionKeys);

        MemoryStream decryptedStream = new MemoryStream();

        await pgpDecrypt.DecryptAsync(
            file.OpenReadStream(),
            decryptedStream
            );

        string filename = file.ContentDisposition.Split(';').FirstOrDefault(f => f.Contains("filename="))?.Split('=')[1].Trim('"') ?? file.Name;

        return new FileContentResult(decryptedStream.ToArray(), file.ContentType) { FileDownloadName = filename.Replace(".pgp", "") };
    }
}
