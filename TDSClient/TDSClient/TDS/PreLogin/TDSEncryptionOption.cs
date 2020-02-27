// Depending upon whether the server has encryption available and enabled, the server responds with an ENCRYPTION value in the response according to the following table

// +=============================+============================================================+===========================================================+================================================================+
// |    Value sent by client     | Value returned by server when server is set to ENCRYPT_OFF | Value returned by server when server is set to ENCRYPT_ON | Value returned by server when server is set to ENCRYPT_NOT_SUP |
// +=============================+============================================================+===========================================================+================================================================+
// | ENCRYPT_OFF                 | ENCRYPT_OFF                                                | ENCRYPT_REQ                                               | ENCRYPT_NOT_SUP                                                |
// +-----------------------------+------------------------------------------------------------+-----------------------------------------------------------+----------------------------------------------------------------+
// | ENCRYPT_ON                  | ENCRYPT_ON                                                 | ENCRYPT_ON                                                | ENCRYPT_NOT_SUP (connection terminated)                        |
// +-----------------------------+------------------------------------------------------------+-----------------------------------------------------------+----------------------------------------------------------------+
// | ENCRYPT_NOT_SUP             | ENCRYPT_NOT_SUP                                            | ENCRYPT_REQ (connection terminated)                       | ENCRYPT_NOT_SUP                                                |
// +-----------------------------+------------------------------------------------------------+-----------------------------------------------------------+----------------------------------------------------------------+
// | ENCRYPT_REQ                 | ENCRYPT_ON                                                 | ENCRYPT_ON                                                | ENCRYPT_NOT_SUP (connection terminated)                        |
// +-----------------------------+------------------------------------------------------------+-----------------------------------------------------------+----------------------------------------------------------------+
// | ENCRYPT_CLIENT_CERT_OFF     | ENCRYPT_OFF                                                | ENCRYPT_REQ                                               | ENCRYPT_NOT_SUP (connection terminated)                        |
// +-----------------------------+------------------------------------------------------------+-----------------------------------------------------------+----------------------------------------------------------------+
// | ENCRYPT_CLIENT_CERT_ON      | ENCRYPT_ON                                                 | ENCRYPT_ON                                                | ENCRYPT_NOT_SUP (connection terminated)                        |
// +-----------------------------+------------------------------------------------------------+-----------------------------------------------------------+----------------------------------------------------------------+
// | ENCRYPT_CLIENT_CERT_NOT_SUP | ENCRYPT_REQ (connection terminated)                        | ENCRYPT_REQ (connection terminated)                       | ENCRYPT_REQ (connection terminated)                            |
// +-----------------------------+------------------------------------------------------------+-----------------------------------------------------------+----------------------------------------------------------------+
// | ENCRYPT_CLIENT_CERT_REQ     | ENCRYPT_ON                                                 | ENCRYPT_ON                                                | ENCRYPT_NOT_SUP (connection terminated)                        |
// +-----------------------------+------------------------------------------------------------+-----------------------------------------------------------+----------------------------------------------------------------+

// Assuming that the client is capable of encryption, the server requires the client to behave in the following manner.

// +=============+===========================================+==========================================+===========================================+===============================================+
// |   Client    | Value returned from server is ENCRYPT_OFF | Value returned from server is ENCRYPT_ON | Value returned from server is ENCRYPT_REQ | Value returned from server is ENCRYPT_NOT_SUP |
// +=============+===========================================+==========================================+===========================================+===============================================+
// | ENCRYPT_OFF | Encrypt login packet only                 | Encrypt entire connection                | Encrypt entire connection                 | No encryption                                 |
// +-------------+-------------------------------------------+------------------------------------------+-------------------------------------------+-----------------------------------------------+
// | ENCRYPT_ON  | Error (connection terminated)             | Encrypt entire connection                | Encrypt entire connection                 | Error (connection terminated)                 |
// +-------------+-------------------------------------------+------------------------------------------+-------------------------------------------+-----------------------------------------------+

namespace TDSClient.TDS.Header
{
    public enum TDSEncryptionOption : byte
    {
        EncryptOff, // Encryption is available but off.
        EncryptOn, // Encryption is available and on.
        EncryptNotSup, // Encryption is not available.
        EncryptReq, // Encryption is required.
        EncryptClientCertOff = 0x80, 
        EncryptClientCertOn = 0x81,
        EncryptClientCertReq = 0x83
    }
}