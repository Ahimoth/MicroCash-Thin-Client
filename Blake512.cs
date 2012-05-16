/*
  BlakeSharp - Blake512
  Public domain implementation of the BLAKE hash algorithm
  by Dominik Reichl <dominik.reichl@t-online.de>
  Web: http://www.dominik-reichl.de/
  If you're using this class, it would be nice if you'd mention
  me somewhere in the documentation of your program, but it's
  not required.

  BLAKE was designed by Jean-Philippe Aumasson, Luca Henzen,
  Willi Meier and Raphael C.-W. Phan.
  BlakeSharp was derived from the reference C implementation.

  Version 1.0 - 2011-11-20
  - Initial release (implementing BLAKE v1.4).
*/

using System;
using System.Security.Cryptography;

namespace BlakeSharp
{
	public sealed class Blake512 : HashAlgorithm
	{
		private ulong[] m_h = new ulong[8];
		private ulong[] m_s = new ulong[4];

		// private ulong[] m_t = new ulong[2];
		private ulong m_t;

		private int m_nBufLen;
		private bool m_bNullT;
		private byte[] m_buf = new byte[128];

		private ulong[] m_v = new ulong[16];
		private ulong[] m_m = new ulong[16];

		private const int NbRounds = 16;

		private static readonly int[] g_sigma = new int[NbRounds * 16] {
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
			14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3,
			11, 8, 12, 0, 5, 2, 15, 13, 10, 14, 3, 6, 7, 1, 9, 4,
			7, 9, 3, 1, 13, 12, 11, 14, 2, 6, 5, 10, 4, 0, 15, 8,
			9, 0, 5, 7, 2, 4, 10, 15, 14, 1, 11, 12, 6, 8, 3, 13,
			2, 12, 6, 10, 0, 11, 8, 3, 4, 13, 7, 5, 15, 14, 1, 9,
			12, 5, 1, 15, 14, 13, 4, 10, 0, 7, 6, 3, 9, 2, 8, 11,
			13, 11, 7, 14, 12, 1, 3, 9, 5, 0, 15, 4, 8, 6, 2, 10,
			6, 15, 14, 9, 11, 3, 0, 8, 12, 2, 13, 7, 1, 4, 10, 5,
			10, 2, 8, 4, 7, 6, 1, 5, 15, 11, 9, 14, 3, 12, 13, 0,
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
			14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3,
			11, 8, 12, 0, 5, 2, 15, 13, 10, 14, 3, 6, 7, 1, 9, 4,
			7, 9, 3, 1, 13, 12, 11, 14, 2, 6, 5, 10, 4, 0, 15, 8,
			9, 0, 5, 7, 2, 4, 10, 15, 14, 1, 11, 12, 6, 8, 3, 13,
			2, 12, 6, 10, 0, 11, 8, 3, 4, 13, 7, 5, 15, 14, 1, 9
		};

		private static readonly ulong[] g_cst = new ulong[16] {
			0x243F6A8885A308D3UL, 0x13198A2E03707344UL, 0xA4093822299F31D0UL,
			0x082EFA98EC4E6C89UL, 0x452821E638D01377UL, 0xBE5466CF34E90C6CUL,
			0xC0AC29B7C97C50DDUL, 0x3F84D5B5B5470917UL, 0x9216D5D98979FB1BUL,
			0xD1310BA698DFB5ACUL, 0x2FFD72DBD01ADFB7UL, 0xB8E1AFED6A267E96UL,
			0xBA7C9045F12C7F99UL, 0x24A19947B3916CF7UL, 0x0801F2E2858EFC16UL,
			0x636920D871574E69UL
		};

		private static readonly byte[] g_padding = new byte[128] {
			0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
		};

		public Blake512()
		{
			this.HashSizeValue = 512; // Hash size in bits
			Initialize();
		}

		/// <summary>
		/// Convert 8 bytes to an <c>UInt64</c> using big-endian.
		/// </summary>
		private static ulong BytesToUInt64(byte[] pb, int iOffset)
		{
			return ((ulong)pb[iOffset + 7] | ((ulong)pb[iOffset + 6] << 8) |
				((ulong)pb[iOffset + 5] << 16) | ((ulong)pb[iOffset + 4] << 24) |
				((ulong)pb[iOffset + 3] << 32) | ((ulong)pb[iOffset + 2] << 40) |
				((ulong)pb[iOffset + 1] << 48) | ((ulong)pb[iOffset] << 56));
		}

		/// <summary>
		/// Convert an <c>UInt64</c> to 8 bytes using big-endian.
		/// </summary>
		private static void UInt64ToBytes(ulong u, byte[] pbOut, int iOffset)
		{
			for(int i = 7; i >= 0; --i)
			{
				pbOut[iOffset + i] = (byte)(u & 0xFF);
				u >>= 8;
			}
		}

		private static ulong RotateRight(ulong u, int nBits)
		{
			return ((u >> nBits) | (u << (64 - nBits)));
		}

		private void G(int a, int b, int c, int d, int r, int i)
		{
			int p = (r << 4) + i;
			int p0 = g_sigma[p];
			int p1 = g_sigma[p + 1];

			m_v[a] += m_v[b] + (m_m[p0] ^ g_cst[p1]);
			m_v[d] = RotateRight(m_v[d] ^ m_v[a], 32);
			m_v[c] += m_v[d];
			m_v[b] = RotateRight(m_v[b] ^ m_v[c], 25);
			m_v[a] += m_v[b] + (m_m[p1] ^ g_cst[p0]);
			m_v[d] = RotateRight(m_v[d] ^ m_v[a], 16);
			m_v[c] += m_v[d];
			m_v[b] = RotateRight(m_v[b] ^ m_v[c], 11);
		}

		private void Compress(byte[] pbBlock, int iOffset)
		{
			for(int i = 0; i < 16; ++i)
				m_m[i] = BytesToUInt64(pbBlock, iOffset + (i << 3));

			Array.Copy(m_h, m_v, 8);
			m_v[8] = m_s[0] ^ 0x243F6A8885A308D3UL;
			m_v[9] = m_s[1] ^ 0x13198A2E03707344UL;
			m_v[10] = m_s[2] ^ 0xA4093822299F31D0UL;
			m_v[11] = m_s[3] ^ 0x082EFA98EC4E6C89UL;
			m_v[12] = 0x452821E638D01377UL;
			m_v[13] = 0xBE5466CF34E90C6CUL;
			m_v[14] = 0xC0AC29B7C97C50DDUL;
			m_v[15] = 0x3F84D5B5B5470917UL;

			if(!m_bNullT)
			{
				m_v[12] ^= m_t;
				m_v[13] ^= m_t;
				// m_v[14] ^= m_t[1];
				// m_v[15] ^= m_t[1];
			}

			for(int r = 0; r < NbRounds; ++r)
			{
				G(0, 4, 8, 12, r, 0);
				G(1, 5, 9, 13, r, 2);
				G(2, 6, 10, 14, r, 4);
				G(3, 7, 11, 15, r, 6);
				G(3, 4, 9, 14, r, 14);
				G(2, 7, 8, 13, r, 12);
				G(0, 5, 10, 15, r, 8);
				G(1, 6, 11, 12, r, 10);
			}

			for(int i = 0; i < 8; ++i) m_h[i] ^= m_v[i];
			for(int i = 0; i < 8; ++i) m_h[i] ^= m_v[i + 8];

			for(int i = 0; i < 4; ++i) m_h[i] ^= m_s[i];
			for(int i = 0; i < 4; ++i) m_h[i + 4] ^= m_s[i];
		}

		public override void Initialize()
		{
			m_h[0] = 0x6A09E667F3BCC908UL;
			m_h[1] = 0xBB67AE8584CAA73BUL;
			m_h[2] = 0x3C6EF372FE94F82BUL;
			m_h[3] = 0xA54FF53A5F1D36F1UL;
			m_h[4] = 0x510E527FADE682D1UL;
			m_h[5] = 0x9B05688C2B3E6C1FUL;
			m_h[6] = 0x1F83D9ABFB41BD6BUL;
			m_h[7] = 0x5BE0CD19137E2179UL;

			Array.Clear(m_s, 0, m_s.Length);

			// Array.Clear(m_t, 0, m_t.Length);
			m_t = 0;

			m_nBufLen = 0;
			m_bNullT = false;

			Array.Clear(m_buf, 0, m_buf.Length);
		}

		protected override void HashCore(byte[] array, int ibStart, int cbSize)
		{
			int iOffset = ibStart;
			int nFill = 128 - m_nBufLen;

			if((m_nBufLen > 0) && (cbSize >= nFill))
			{
				Array.Copy(array, iOffset, m_buf, m_nBufLen, nFill);
				m_t += 1024;
				Compress(m_buf, 0);
				iOffset += nFill;
				cbSize -= nFill;
				m_nBufLen = 0;
			}

			while(cbSize >= 128)
			{
				m_t += 1024;
				Compress(array, iOffset);
				iOffset += 128;
				cbSize -= 128;
			}

			if(cbSize > 0)
			{
				Array.Copy(array, iOffset, m_buf, m_nBufLen, cbSize);
				m_nBufLen += cbSize;
			}
			else m_nBufLen = 0;
		}

		protected override byte[] HashFinal()
		{
			byte[] pbMsgLen = new byte[16];
			UInt64ToBytes(m_t + ((ulong)m_nBufLen << 3), pbMsgLen, 8);

			if(m_nBufLen == 111)
			{
				m_t -= 8;
				HashCore(new byte[1] { 0x81 }, 0, 1);
			}
			else
			{
				if(m_nBufLen < 111)
				{
					if(m_nBufLen == 0) m_bNullT = true;
					m_t -= 888UL - ((ulong)m_nBufLen << 3);
					HashCore(g_padding, 0, 111 - m_nBufLen);
				}
				else
				{
					m_t -= 1024UL - ((ulong)m_nBufLen << 3);
					HashCore(g_padding, 0, 128 - m_nBufLen);
					m_t -= 888UL;
					HashCore(g_padding, 1, 111);
					m_bNullT = true;
				}
				HashCore(new byte[1] { 0x01 }, 0, 1);
				m_t -= 8;
			}

			m_t -= 128;
			HashCore(pbMsgLen, 0, 16);

			byte[] pbDigest = new byte[64];
			for(int i = 0; i < 8; ++i)
				UInt64ToBytes(m_h[i], pbDigest, i << 3);
			return pbDigest;
		}
	}
}
