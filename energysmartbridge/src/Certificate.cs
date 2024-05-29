using System;
using System.Security.Cryptography.X509Certificates;

internal static class Certificate {
  public static readonly X509Certificate2 SelfSignedCertificate;

  static Certificate() {
    // A base64-encoded, pfx-format certificate and associatd private key for
    // energysmartwaterheater.com which expires in the year 2034. This certificate is self-signed
    // and uses an obsolete SHA-1 hash, so it should not be accepted by any sane TLS
    // implementation, but fortunately this particular IOT device doesn't actually bother to check
    // the certificate chain and it in fact requires an obsolete SHA-1 hash.
    // We hard-code this certificate because .NET can't actually create a certificate with a SHA-1
    // hash.
    const string certBase64 = """
        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
      """;
    SelfSignedCertificate = new X509Certificate2(Convert.FromBase64String(certBase64));
  }
}