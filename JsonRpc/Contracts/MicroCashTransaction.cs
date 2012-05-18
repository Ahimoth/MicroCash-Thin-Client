using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace MicroCash.Client.Thin.JsonRpc.Contracts
{
    public class MicroCashTransaction
    {
        public uint m_dwType;
        public uint m_dwAddressID;
        public byte[] m_FromAddress;
        public byte[] m_RecvAddress;
        public byte[] m_Info;
        public Int64 m_qAmount;
        public byte[] m_Signature;
        public byte[] m_Extra1;
        public byte[] m_Extra2;

        public MicroCashTransaction()
        {
            m_Info = new byte[8];
            m_FromAddress = new byte[10];
            m_RecvAddress = new byte[10];
            m_Signature = new byte[64];
            m_Extra1 = new byte[64];
            m_Extra2 = new byte[64];
        }

        public byte[] GetByteBuffer(bool bIncludeSig)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(m_dwType);
            bw.Write(m_dwAddressID);
            bw.Write(m_FromAddress);
            bw.Write(m_RecvAddress);
            bw.Write(m_Info);
            bw.Write(m_qAmount);
            if (m_dwType != 0)
            {
                bw.Write(m_Extra1);
                bw.Write(m_Extra2);
            }
            if (bIncludeSig) bw.Write(m_Signature);
            return ms.ToArray();
        }

        public byte[] GetHash(bool bIncludeSig)
        {

            SHA256 shaM = new SHA256Managed();
            byte[] result = shaM.ComputeHash(GetByteBuffer(bIncludeSig));
            byte[] result2 = shaM.ComputeHash(result);
            return result2;
        }
    }
}
