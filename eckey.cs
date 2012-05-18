/*
 * MicroCash Thin Client
 * Please see License.txt for applicable copyright and licensing details.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Math.EC;
using BlakeSharp;
namespace MicroCash.Client.Thin
{    

    public class CMicroCashKeyPair
    {
        public BigInteger m_Priv;
        public byte[] m_Pub;
        public byte[] m_Address;
        public string m_PrivKeyString;
        public string m_PubKeyString;
        
        /** Generates an entirely new keypair. */
        public CMicroCashKeyPair(bool bGenerate)
        {
            if(bGenerate)
            {
                ECKeyPairGenerator gen = new ECKeyPairGenerator();
                var secureRandom = new SecureRandom();
                var ps = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("secp256k1");
                var ecParams = new ECDomainParameters(ps.Curve, ps.G, ps.N, ps.H);
                var keyGenParam = new ECKeyGenerationParameters(ecParams, secureRandom);
                gen.Init(keyGenParam);

                AsymmetricCipherKeyPair kp = gen.GenerateKeyPair();

                ECPrivateKeyParameters priv = (ECPrivateKeyParameters)kp.Private;
                ECPublicKeyParameters pub = (ECPublicKeyParameters)kp.Public;

                byte[] hexpriv = priv.D.ToByteArrayUnsigned();
                //byte[] hexPub = pub
                Org.BouncyCastle.Math.BigInteger Db = new Org.BouncyCastle.Math.BigInteger(1,hexpriv);
                ECPoint dd = ps.G.Multiply(Db);

                byte[] pubaddr = new byte[65];
                byte[] Y = dd.Y.ToBigInteger().ToByteArray();
                Array.Copy(Y, 0, pubaddr, 64 - Y.Length + 1, Y.Length);
                byte[] X = dd.X.ToBigInteger().ToByteArray();
                Array.Copy(X, 0, pubaddr, 32 - X.Length + 1, X.Length);
                pubaddr[0] = 4;


                m_Priv = new BigInteger(hexpriv);
                m_Pub = pubaddr;
                m_Address = ConvertPubKeyToAddress(m_Pub);
                m_PubKeyString = bytesToHexString(m_Pub);
                m_PrivKeyString = bytesToHexString(m_Priv.ToByteArray());
            }
        }

        public void SetKeyByString(string priv, string pub)
        {
            m_Priv = new BigInteger(1,HexString2Bytes(priv));
            m_Pub = HexString2Bytes(pub);
            m_Address = ConvertPubKeyToAddress(m_Pub);
            m_PubKeyString = bytesToHexString(m_Pub);
            m_PrivKeyString = bytesToHexString(m_Priv.ToByteArray());
        }
              
        private static byte[] ConvertPubKeyToAddress(byte[] bytes)
        {
            byte[] messageANDbytes = new byte[64 + bytes.Length];

            using (Blake512 blake512 = new Blake512())
            {

                //first compute blake hash of pubkey
                byte[] pbHash = blake512.ComputeHash(bytes);

                //then copy into our "Message and hash" buffer
                Array.Copy(pbHash, 0, messageANDbytes, 0, 64);
                Array.Copy(bytes, 0, messageANDbytes, 64, bytes.Length);

                //do some rounds of blake512
                for (int x = 0; x < 100; x++)
                {
                    pbHash = blake512.ComputeHash(messageANDbytes); //compute hash
                    Array.Copy(pbHash, 0, messageANDbytes, 0, 64);  //copy new hash into it
                }
            }

            using (SHA256 hash256 = SHA256.Create())
            {
                hash256.Initialize();
                hash256.TransformFinalBlock(messageANDbytes, 0, messageANDbytes.Length);

                byte[] address = new byte[10];
                Array.Copy(hash256.Hash, 0, address, 0, 10);
                return address;
            }
        }

        public string GetAddressString()
        {
            MicroCashAddress outaddr = new MicroCashAddress(m_Address);
            return outaddr.GetAddressString();
        }

        public byte[] GetAddressBytes()
        {
            return m_Address;
        }

        internal static byte[] HexString2Bytes(string hexString)
        {
            //check for null
            if (hexString == null) return null;
            //get length
            int len = hexString.Length;
            if (len % 2 == 1) return null;
            int len_half = len / 2;
            //create a byte array
            byte[] bs = new byte[len_half];
            //convert the hexstring to bytes
            for (int i = 0; i != len_half; i++)
            {
                bs[i] = (byte)Int32.Parse(hexString.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            //return the byte array
            return bs;
        }

        public static String bytesToHexString(byte[] bytes)
        {
            StringBuilder buf = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                String s = ((int)0xFF & b).ToString("x");//  Integer.toString(0xFF & b, 16);    // TODO: correct?
                if (s.Length < 2)
                {
                    buf.Append('0');
                }
                buf.Append(s);
            }
            return buf.ToString();
        }

        public byte[] Sign(byte[] data)
        {
            ECDsaSigner signer = new ECDsaSigner();
            ECDsaSigner signerverify = new ECDsaSigner();
            var ps = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("secp256k1");
            ECDomainParameters ecParams = new ECDomainParameters(ps.Curve, ps.G, ps.N, ps.H);

            ECPrivateKeyParameters privKey = new ECPrivateKeyParameters(m_Priv, ecParams);
            ECPublicKeyParameters pubKey = new ECPublicKeyParameters(ecParams.Curve.DecodePoint(m_Pub), ecParams);

            signer.Init(true, privKey);
            signerverify.Init(false, pubKey);

            BigInteger[] sigs = signer.GenerateSignature(data);
            //sigs[0].BitLength sigs[1].BitLength

            bool bValid = signerverify.VerifySignature(data, sigs[0], sigs[1]);


            byte[] sig = new byte[64];
            for (int x = 0; x < 64; x++) sig[x] = 0;
            byte[] sig1 = sigs[0].ToByteArrayUnsigned();
            byte[] sig2 = sigs[1].ToByteArrayUnsigned();
            Array.Copy(sig1,0, sig,  0 + (32 - sig1.Length), sig1.Length);
            Array.Copy(sig2,0, sig, 32 + (32 - sig2.Length), sig2.Length);            
            return sig;     
            
            /*
                MemoryStream ms = new MemoryStream();
                DerSequenceGenerator seq = new DerSequenceGenerator(ms);
                seq.AddObject(new DerInteger(sigs[0]));
                seq.AddObject(new DerInteger(sigs[1]));
                seq.Close();
                return ms.ToArray();
             */
        }
    }


    public class MicroCashAddress
    {
        private byte[] m_Address80;
        private byte[] m_Info;
        private int m_nAmount;
        private bool m_bIsPaymentCode;
        private bool m_bIsValid;
        private bool m_bIsLongAddress;

        public MicroCashAddress()
        {
            m_bIsValid = false;
            m_bIsPaymentCode = false;
            m_bIsLongAddress = false;
            m_Address80 = null;
        }
        public MicroCashAddress(byte[] address10bytes)
        {
            m_bIsValid = false;
            m_bIsPaymentCode = false;
            m_bIsLongAddress = false;
            m_Address80 = new byte[10];
            Array.Copy(address10bytes, m_Address80, 10);
        }
        public MicroCashAddress(string address)
        {
            m_bIsValid = false;
            m_bIsPaymentCode = false;
            m_bIsLongAddress = false;
            SetString(address);
        }

        public bool IsPaymentCode { get { return m_bIsPaymentCode; } }
        public bool IsLongAddress() { return m_bIsLongAddress; }
        public bool IsValid() { return m_bIsValid; }
        public byte[] GetInfoBytes() { return m_Info; }
        public byte[] GetAddressBytes() { return m_Address80; }
        public string GetAddressString() { return EncodeAddress(false,false); }
        public string GetAddressInfoString() { return EncodeAddress(true,false); }
        public string GetPaymentCodeString() { return EncodeAddress(false,true); }
        public Int64 GetPaymentAmount() { return (Int64)m_nAmount; }
        public string GetPaymentAmountInfoString()
        {
            string ret="";
            foreach (Byte b in m_Info)
            {
                if(b>=32 && b<127) ret+= (char)b;
                else ret+=" ";                
            }
            return ret;
        }
                    

        public string EncodeAddress(bool bLongAddress, bool bPayMentCode)
        {
            if (m_Address80 == null) return "";
            //checksum
            SHA256 hash256 = SHA256.Create();
            hash256.Initialize();

            byte[] CheckBytes = null;
            string checkaddr = ""; 

            string inaddr = EncodeBase32(m_Address80);
            string info = "";
            string amount = "";
            if (bLongAddress)
            {
                byte[] infobytes = new byte[8];
                Array.Copy(m_Info, infobytes, 8);
                info = EncodeBase32(infobytes);
                CheckBytes = new byte[18];
                Array.Copy(m_Address80, 0, CheckBytes, 0, 10);
                Array.Copy(infobytes, 0, CheckBytes, 10, 8);
            }
            else if (bPayMentCode)
            {
                byte[] infobytes = new byte[8];
                Array.Copy(m_Info, infobytes, 8);
                info = EncodeBase32(infobytes);               

                byte[] amountbytes = new byte[4];
                amountbytes[0] = (byte)((m_nAmount >> 24) & 0xFF);
                amountbytes[1] = (byte)((m_nAmount >> 16) & 0xFF);
                amountbytes[2] = (byte)((m_nAmount >> 8) & 0xFF);
                amountbytes[3] = (byte)((m_nAmount >> 0) & 0xFF);
                amount = "-" + EncodeBase32(amountbytes);

                CheckBytes = new byte[22];
                Array.Copy(m_Address80, 0, CheckBytes, 0, 10);
                Array.Copy(infobytes, 0, CheckBytes, 10, 8);
                Array.Copy(amountbytes, 0, CheckBytes, 18, 4);
                
            }
            else
            {
                CheckBytes = new byte[10];
                Array.Copy(m_Address80, 0, CheckBytes, 0, 10);                
                
            }
            hash256.TransformFinalBlock(CheckBytes, 0, CheckBytes.Length);
            checkaddr = EncodeChar(hash256.Hash[0]);
            return "micro(" + inaddr + info + amount + checkaddr +")cash";

        }
        public void SetInfo(byte[] info)
        {
            m_Info = new byte[8];
            for (int x = 0; x < 8; x++) m_Info[x] = 0;

            int nLen = info.Length;
            if(nLen>8) nLen=8;
            Array.Copy(info,m_Info,nLen);
        }
        public void SetAmount(Int64 amount)
        {
            m_nAmount = (int)(amount & 0xFFFFFFFF);
        }
        public bool SetString(string address)
        {
            int nStart = address.IndexOf("micro(");
            if (nStart >= 0)    nStart += 6;
            if (nStart < 0)
            {
                nStart = address.IndexOf("icro(");
                if (nStart >= 0) nStart += 5;
            }
            if (nStart < 0)
            {
                nStart = address.IndexOf("cro(");
                if (nStart >= 0) nStart += 4;
            }
            if (nStart < 0)
            {
                nStart = address.IndexOf("ro(");
                if (nStart >= 0) nStart += 3;
            }
            if (nStart < 0)
            {
                nStart = address.IndexOf("o(");
                if (nStart >= 0) nStart += 2;
            }

            int nEnd = address.IndexOf(")cash");
            if(nEnd<0) nEnd = address.IndexOf(")as");
            if(nEnd<0) nEnd = address.IndexOf(")a");
            if(nEnd<0) nEnd = address.IndexOf(")");
            if(nEnd<0) return false;
            
            string inner = address.Substring(nStart, nEnd-nStart);
            if (inner.Length != 17 && inner.Length != 30 && inner.Length != 38) return false;
                        
            //decode the actual address and check its checksum
            byte[] checkBytes = null;
            byte[] infoBytes = null;
            byte[] addrbytes = DecodeBase32(inner.Substring(0,16));
            if(addrbytes==null || addrbytes.Length != 10) return false;
            m_Address80=addrbytes;
            
            //see if we have a payment code or longer address and make sure its valid too if we do
            byte checksumByte = 0xFF;
            if (inner.Length == 17)
            {
                
                m_Info = null;
                m_nAmount = 0;
                checkBytes = new byte[10];
                Array.Copy(addrbytes, checkBytes, 10);
                checksumByte = DecodeChar(inner[16]);
            }
            if (inner.Length == 30 || inner.Length == 38)
            {
                infoBytes = DecodeBase32(inner.Substring(16, 13));
                m_Info = new byte[8];
                Array.Copy(infoBytes, m_Info, 8);
            }

            if (inner.Length == 30)
            {
                m_nAmount = 0;
                checkBytes = new byte[18];
                Array.Copy(addrbytes,0, checkBytes,0, 10);
                Array.Copy(infoBytes, 0, checkBytes, 10, 8);
                checksumByte = DecodeChar(inner[29]);
                m_bIsLongAddress = true;                
            }
            else if (inner.Length == 38)
            {
                if (inner[29] != '-') return false;
                byte[] bytes = DecodeBase32(inner.Substring(30, 7));
                if (bytes.Length != 5) return false;   //we actually only have 4 bytes in there, but 7 characters ends up as 4.3 bytes
                
                m_nAmount = (int)(((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | ((uint)bytes[3] << 0));
                m_bIsPaymentCode = true;

                checkBytes = new byte[22];
                Array.Copy(addrbytes, 0, checkBytes, 0, 10);
                Array.Copy(infoBytes, 0, checkBytes, 10, 8);
                Array.Copy(bytes, 0, checkBytes, 18, 4);
                checksumByte = DecodeChar(inner[37]);
            }


            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] checksum = sha256.ComputeHash(checkBytes, 0, checkBytes.Length);
            if (checksumByte != (checksum[0] & 0x1F)) return false;
            m_bIsValid = true;
            return true;
        }


        private static string _base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        
        public static string EncodeChar(byte input)
        {
            return _base32Alphabet[input & 0x1F].ToString();    //only have 5 bits to work with in a byte
        }
        
        public static String EncodeBase32(byte[] input)
        {
            int nBitCount = 0;
            string ret="";
            uint dwBits = 0;
            foreach (byte b in input)
            {
                dwBits <<= 8;
                dwBits |= b;
                nBitCount += 8;
                while (nBitCount >= 5)
                {
                    ret += _base32Alphabet[(int)((dwBits >> (nBitCount - 5)) & 0x1F)];
                    nBitCount -= 5;
                }
            }
            if (nBitCount > 0) ret += _base32Alphabet[(int)((dwBits & ((1<<nBitCount)-1)) & 0x1F)];
            return ret;         
        }

        public static byte DecodeChar(char b)
        {
            int n = _base32Alphabet.IndexOf(b);
            if (n == -1) return 0xFF;
            return (byte)n;
        }

        public static byte[] AddByte(byte[] inarray, byte data)
        {
            byte[] bb;
            if (inarray == null) bb = new byte[1];
            else
            {
                bb = new byte[inarray.Length + 1];
                Array.Copy(inarray, 0, bb, 0, inarray.Length);                                
            }
            bb[bb.Length - 1] = data;
            return bb;
        }
        public static byte[] DecodeBase32(string address)
        {
            uint dwBits = 0;
            int nBitCount=0;
            byte[] bb = null;
            foreach (char c in address)
            {
                int n = _base32Alphabet.IndexOf(c);
                if( n == -1) return null;
                dwBits <<= 5;
                dwBits |= (byte)n;
                nBitCount += 5;
                if (nBitCount >= 8)
                {
                    bb = AddByte(bb,(byte)((dwBits>>(nBitCount-8))&0xFF));
                    nBitCount -= 8;
                    dwBits &= (uint)((1 << nBitCount) - 1); //clear bits we just saved
                }
            }
            if (nBitCount != 0) bb = AddByte(bb, (byte)(dwBits & 0xFF));    //add any left over bits to the next byte
            return bb;
        }

        
    }

        

    /**
     * Represents an elliptic curve keypair that we own and can use for signing transactions. Currently,
     * Bouncy Castle is used. In future this may become an interface with multiple implementations using different crypto
     * libraries. The class also provides a static method that can verify a signature with just the public key.<p>
     */
    //public class ECKey /*implements Serializable */
    //{
    //    //private static readonly ECDomainParameters ecParams;

    //    //private static readonly SecureRandom secureRandom;
    //    private const long serialVersionUID = -728224901792295832L;
    //    private static RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
    //    //private ECDsaCng generator = new ECDsaCng();

    //    ////static {
    //    //    // All clients must agree on the curve to use by agreement. BitCoin uses secp256k1.
    //    //private static X9ECParameters params = SECNamedCurves.getByName("secp256k1");
    //    //    private static ecParams = new ECDomainParameters(params.getCurve(), params.getG(), params.getN(),  params.getH());
    //    //    private static secureRandom = new SecureRandom();
    //    ////}

    //    private readonly BigInteger priv;
    //    private readonly byte[] pub;

    //    private byte[] pubKeyHash;

    //    /** Generates an entirely new keypair. */
    //    public ECKey()
    //    {
    //        ECDsaCng generator = new ECDsaCng(256);
    //        generator.HashAlgorithm = CngAlgorithm.ECDsaP256;
    //        priv = new BigInteger(generator.Key.Export(CngKeyBlobFormat.EccPublicBlob));
    //        pub = generator.Key.Export(CngKeyBlobFormat.EccPublicBlob);
    //        //char[] outArray = new char[200];
    //        //int ret = Convert.ToBase64CharArray(pub, 0, pub.Length, outArray, 0);
    //        //string str = Convert.ToBase64String(pub);
    //        //generator.KeySize = generator.LegalKeySizes
    //        //ECKeyGenerationParameters keygenParams = new ECKeyGenerationParameters(ecParams, secureRandom);
    //        //generator.init(keygenParams);
    //        //AsymmetricCipherKeyPair keypair = generator.generateKeyPair();
    //        //ECPrivateKeyParameters privParams = (ECPrivateKeyParameters) keypair.getPrivate();
    //        //ECPublicKeyParameters pubParams = (ECPublicKeyParameters) keypair.getPublic();
    //        //priv = privParams.getD();
    //        //// The public key is an encoded point on the elliptic curve. It has no meaning independent of the curve.
    //        //pub = pubParams.getQ().getEncoded();
    //    }

    //    /**
    //     * Construct an ECKey from an ASN.1 encoded private key. These are produced by OpenSSL and stored by the BitCoin
    //     * reference implementation in its wallet.
    //     */
    //    public static ECKey fromASN1(byte[] asn1privkey)
    //    {
    //        throw new Exception("NYI");
    //        //return new ECKey(extractPrivateKeyFromASN1(asn1privkey));
    //    }

    //    /**
    //     * Output this ECKey as an ASN.1 encoded private key, as understood by OpenSSL or used by the BitCoin reference
    //     * implementation in its wallet storage format.
    //     */
    //    public byte[] toASN1()
    //    {
    //        throw new Exception("NYI");
    //        //try {
    //        //    ByteArrayOutputStream baos = new ByteArrayOutputStream(400);
    //        //    ASN1OutputStream encoder = new ASN1OutputStream(baos);

    //        //    // ASN1_SEQUENCE(EC_PRIVATEKEY) = {
    //        //    //   ASN1_SIMPLE(EC_PRIVATEKEY, version, LONG),
    //        //    //   ASN1_SIMPLE(EC_PRIVATEKEY, privateKey, ASN1_OCTET_STRING),
    //        //    //   ASN1_EXP_OPT(EC_PRIVATEKEY, parameters, ECPKPARAMETERS, 0),
    //        //    //   ASN1_EXP_OPT(EC_PRIVATEKEY, publicKey, ASN1_BIT_STRING, 1)
    //        //    // } ASN1_SEQUENCE_END(EC_PRIVATEKEY)
    //        //    DERSequenceGenerator seq = new DERSequenceGenerator(encoder);
    //        //    seq.addObject(new DERInteger(1)); // version
    //        //    seq.addObject(new DEROctetString(priv.toByteArray()));
    //        //    seq.addObject(new DERTaggedObject(0, SECNamedCurves.getByName("secp256k1").getDERObject()));
    //        //    seq.addObject(new DERTaggedObject(1, new DERBitString(getPubKey())));
    //        //    seq.close();
    //        //    encoder.close();
    //        //    return baos.toByteArray();
    //        //} catch (IOException e) {
    //        //    throw new RuntimeException(e);  // Cannot happen, writing to memory stream.
    //        //}
    //    }

    //    /**
    //     * Creates an ECKey given only the private key. This works because EC public keys are derivable from their
    //     * private keys by doing a multiply with the generator value.
    //     */
    //    public ECKey(BigInteger privKey)
    //    {
    //        this.priv = privKey;
    //        this.pub = publicKeyFromPrivate(privKey);
    //    }

    //    /** Derive the public key by doing a point multiply of G * priv. */
    //    private static byte[] publicKeyFromPrivate(BigInteger privKey)
    //    {
    //        throw new Exception("NYI");
    //        //return ecParams.getG().multiply(privKey).getEncoded();
    //    }

    //    /** Gets the hash160 form of the public key (as seen in addresses). */
    //    /*
    //    public byte[] getPubKeyHash()
    //    {
    //        if (pubKeyHash == null)
    //        {
    //            pubKeyHash = Utils.sha256hash160(this.pub);
    //        }
    //        return pubKeyHash;
    //    }
    //    */
    //    /**
    //     * Gets the raw public key value. This appears in transaction scriptSigs. Note that this is <b>not</b> the same
    //     * as the pubKeyHash/address.
    //     */
    //    public byte[] getPubKey()
    //    {
    //        return pub;
    //    }

    //    /*
    //    public override string ToString()
    //    {
    //        StringBuilder b = new StringBuilder();
    //        b.Append("pub:").Append(Utils.bytesToHexString(pub));
    //        b.Append(" priv:").Append(Utils.bytesToHexString(priv.ToByteArray()));
    //        return b.ToString();
    //    }
    //     * */

    //    /**
    //     * Returns the address that corresponds to the public part of this ECKey. Note that an address is derived from
    //     * the RIPEMD-160 hash of the public key and is not the public key itself (which is too large to be convenient).
    //     */
    //    /*
    //    public Address toAddress(NetworkParameters param)
    //    {
    //        byte[] hash160 = Utils.sha256hash160(pub);
    //        return new Address(param, hash160);
    //    }
    //     * */

    //    /**
    //     * Calcuates an ECDSA signature in DER format for the given input hash. Note that the input is expected to be
    //     * 32 bytes long.
    //     */
    //    public byte[] sign(byte[] input)
    //    {
    //        throw new Exception("NYI");
    //        //ECDSASigner signer = new ECDSASigner();
    //        //ECPrivateKeyParameters privKey = new ECPrivateKeyParameters(priv, ecParams);
    //        //signer.init(true, privKey);
    //        //BigInteger[] sigs = signer.generateSignature(input);
    //        //// What we get back from the signer are the two components of a signature, r and s. To get a flat byte stream
    //        //// of the type used by BitCoin we have to encode them using DER encoding, which is just a way to pack the two
    //        //// components into a structure.
    //        //try {
    //        //    ByteArrayOutputStream bos = new ByteArrayOutputStream();
    //        //    DERSequenceGenerator seq = new DERSequenceGenerator(bos);
    //        //    seq.addObject(new DERInteger(sigs[0]));
    //        //    seq.addObject(new DERInteger(sigs[1]));
    //        //    seq.close();
    //        //    return bos.toByteArray();
    //        //} catch (IOException e) {
    //        //    throw new RuntimeException(e);  // Cannot happen.
    //        //}
    //    }

    //    /**
    //     * Verifies the given ASN.1 encoded ECDSA signature against a hash using the public key.
    //     * @param data Hash of the data to verify.
    //     * @param signature ASN.1 encoded signature.
    //     * @param pub The public key bytes to use.
    //     */
    //    public static bool verify(byte[] data, byte[] signature, byte[] pub)
    //    {
    //        throw new Exception("NYI");
    //        //ECDSASigner signer = new ECDSASigner();
    //        //ECPublicKeyParameters params = new ECPublicKeyParameters(ecParams.getCurve().decodePoint(pub), ecParams);
    //        //signer.init(false, params);
    //        //try {
    //        //    ASN1InputStream decoder = new ASN1InputStream(signature);
    //        //    DERSequence seq = (DERSequence) decoder.readObject();
    //        //    DERInteger r = (DERInteger) seq.getObjectAt(0);
    //        //    DERInteger s = (DERInteger) seq.getObjectAt(1);
    //        //    decoder.close();
    //        //    return signer.verifySignature(data, r.getValue(), s.getValue());
    //        //} catch (IOException e) {
    //        //    throw new RuntimeException(e);
    //        //}
    //    }

    //    /**
    //     * Verifies the given ASN.1 encoded ECDSA signature against a hash using the public key.
    //     * @param data Hash of the data to verify.
    //     * @param signature ASN.1 encoded signature.
    //     */
    //    public bool verify(byte[] data, byte[] signature)
    //    {
    //        return ECKey.verify(data, signature, pub);
    //    }

    //    private static BigInteger extractPrivateKeyFromASN1(byte[] asn1privkey)
    //    {
    //        // To understand this code, see the definition of the ASN.1 format for EC private keys in the OpenSSL source
    //        // code in ec_asn1.c:
    //        //
    //        // ASN1_SEQUENCE(EC_PRIVATEKEY) = {
    //        //   ASN1_SIMPLE(EC_PRIVATEKEY, version, LONG),
    //        //   ASN1_SIMPLE(EC_PRIVATEKEY, privateKey, ASN1_OCTET_STRING),
    //        //   ASN1_EXP_OPT(EC_PRIVATEKEY, parameters, ECPKPARAMETERS, 0),
    //        //   ASN1_EXP_OPT(EC_PRIVATEKEY, publicKey, ASN1_BIT_STRING, 1)
    //        // } ASN1_SEQUENCE_END(EC_PRIVATEKEY)
    //        //

    //        throw new Exception("NYI");
    //        //try {
    //        //    ASN1InputStream decoder = new ASN1InputStream(asn1privkey);
    //        //    DERSequence seq = (DERSequence) decoder.readObject();
    //        //    assert seq.size() == 4 : "Input does not appear to be an ASN.1 OpenSSL EC private key";
    //        //    assert ((DERInteger) seq.getObjectAt(0)).getValue().equals(BigInteger.ONE) : "Input is of wrong version";
    //        //    DEROctetString key = (DEROctetString) seq.getObjectAt(1);
    //        //    decoder.close();
    //        //    return new BigInteger(key.getOctets());
    //        //} catch (IOException e) {
    //        //    throw new RuntimeException(e);  // Cannot happen, reading from memory stream.
    //        //}
    //    }
    //}
}