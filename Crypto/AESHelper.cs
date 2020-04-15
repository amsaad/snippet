using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Amsaad.Lib.Security
{
	/// <summary>
	/// Symmetric keys are good for encrypting large amounts of data
	/// </summary>
	public class AESHelper
	{
		private static int keySizes = 256;
		private static int blockSize = 128;
		private static PaddingMode pMode = PaddingMode.PKCS7;
		private static CipherMode cMode = CipherMode.ECB;
		private static byte[] key = GenEncryptionKey();
		//! long as possible
		//! https://travistidwell.com/jsencrypt/demo/ to generate RAS key 
		private const string passphrase = @"MFswDQYJKoZIhvcNAQEBBQADSgAwRwJAX7LWF0aG6mFMw2dy9mZVKjOv94Qh6lhKZ9Kao/u7AWN+wELX3igY4dK2Au47yw/13S5yO+35xZHpkt5MxTDRWQIDAQAB";



		public static string Encrypt(string plainTXT)
		{
			string cipherText = "";

			if (string.IsNullOrEmpty(plainTXT) || plainTXT.Length < 1)
				throw new ArgumentNullException();
			byte[] plainBytes = Encoding.Unicode.GetBytes(plainTXT);
			//using (Rfc2898DeriveBytes pass = new Rfc2898DeriveBytes(passphrase, salt))
			//{
			using (var cryptor = CreateCryptor())
			{
				ICryptoTransform encryptor = cryptor.CreateEncryptor();
				using (MemoryStream ms = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
					{
						cs.Write(plainBytes, 0, plainBytes.Length);

					}
					byte[] cipherBytes = ms.ToArray();
					cipherText = Convert.ToBase64String(cipherBytes);
				}
				cryptor.Clear();
			}
			return cipherText;
			//}
		}



		public static string Decrypt(string encTXT)
		{
			var data = Convert.FromBase64String(encTXT);
			byte[] cipherBytes = new byte[data.Length];
			string plainText = "";

			using (var crypto = CreateCryptor())
			{
				ICryptoTransform Dec = crypto.CreateDecryptor();
				using (MemoryStream ms = new MemoryStream(data))
				{
					using (var cs = new CryptoStream(ms, Dec, CryptoStreamMode.Read))
					{
						cs.Read(cipherBytes, 0, cipherBytes.Length);
						plainText = Encoding.Unicode.GetString(cipherBytes.ToArray());
					}
				}
				crypto.Clear();
			}

			return plainText;
		}

		//! code v1
		//public static string Encrypt(string DataVal)
		//{
		//	string EncData = string.Empty;

		//	byte[] dataInBytes = Encoding.Unicode.GetBytes(DataVal);

		//	SymmetricAlgorithm c = Aes.Create();
		//	HashAlgorithm h = MD5.Create();
		//	c.BlockSize = 128;
		//	c.Padding = PaddingMode.Zeros;
		//	c.IV = GetIV();
		//	c.Key = h.ComputeHash(Encoding.Unicode.GetBytes("amsaad"));
		//	using (var ms = new MemoryStream())
		//	{
		//		using (var cs = new CryptoStream(ms, c.CreateEncryptor(), CryptoStreamMode.Write))
		//		{
		//			cs.Write(dataInBytes, 0, dataInBytes.Length);
		//			EncData = Convert.ToBase64String(ms.ToArray());
		//		}

		//	}
		//	return EncData;
		//}

		//public static string Decrypt(string DataVal)
		//{
		//	string DecData = string.Empty;

		//	byte[] dataInBytes = Convert.FromBase64String(DataVal);

		//	SymmetricAlgorithm c = Aes.Create();
		//	HashAlgorithm h = MD5.Create();
		//	c.BlockSize = 128;
		//	c.Padding = PaddingMode.Zeros;
		//	c.IV = GetIV();
		//	c.Key = h.ComputeHash(Encoding.Unicode.GetBytes("amsaad"));
		//	using (var ms = new MemoryStream(dataInBytes))
		//	{
		//		using (var cs = new CryptoStream(ms, c.CreateDecryptor(), CryptoStreamMode.Read))
		//		{
		//			byte[] decryptedData = new byte[dataInBytes.Length];
		//			cs.Read(decryptedData, 0, decryptedData.Length);
		//			DecData = Encoding.UTF8.GetString(decryptedData);
		//		}

		//	}
		//	return DecData;
		//}


		#region Helper

		private static byte[] GenEncryptionKey()
		{
			//TODO: Should get RAS key --> RAS Helper is not ready!!
			//! Check and test key generation on both Encrypt and decrypt 
			HashAlgorithm hash = MD5.Create();
			return hash.ComputeHash(Encoding.Unicode.GetBytes(passphrase));
		}

		private static AesManaged CreateCryptor()
		{
			AesManaged cryptor = new AesManaged();
			cryptor.GenerateKey();
			cryptor.KeySize = keySizes;
			cryptor.BlockSize = blockSize;
			cryptor.Padding = pMode;
			cryptor.Key = key;
			cryptor.Mode = cMode;
			cryptor.GenerateIV();
			return cryptor;
		}

		private static byte[] GetIV()
		{
			byte[] iv;
			using (var aes = new AesCryptoServiceProvider())
			{
				aes.GenerateIV();
				iv = aes.IV;
			}
			return iv;
		}
		#endregion
	}
}
