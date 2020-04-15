using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Amsaad.Lib.Security
{
	/// <summary>
	/// asymmetric keys are better for small chunks
	/// </summary>
  //! NOT TESTED AFTER APPLY Sept,2019 CHANGES
	public class RASHelper
	{
		//! 1024, 2048
		const int keySize = 512;
		/// <summary>
		/// to generate private&public key 
		/// </summary>
		/// <returns></returns>
		public static Tuple<string, string> GenPPKeys()
		{
			
			var rsaSrvs = new RSACryptoServiceProvider(keySize);

			string publicKey = Convert.ToBase64String(rsaSrvs.ExportCspBlob(false));
			string privateKey = Convert.ToBase64String(rsaSrvs.ExportCspBlob(true));

			return new Tuple<string, string>(privateKey, publicKey);
		}

		public static string Encrypt(string publicKey, string plainTXT)
		{
			if (string.IsNullOrEmpty(publicKey))
				throw new ArgumentNullException("publicKey");

			var rsaSrvs = new RSACryptoServiceProvider();

			rsaSrvs.ImportCspBlob(Convert.FromBase64String(publicKey));

			byte[] plainBytes = Encoding.UTF8.GetBytes(plainTXT);
			byte[] encryptedBytes = rsaSrvs.Encrypt(plainBytes, false);

			return Convert.ToBase64String(encryptedBytes);
		}
		public static byte[] EncryptB(string publicKey, string plainTXT)
		{
			if (string.IsNullOrEmpty(publicKey))
				throw new ArgumentNullException("publicKey");

			var rsaSrvs = new RSACryptoServiceProvider();

			rsaSrvs.ImportCspBlob(Convert.FromBase64String(publicKey));

			byte[] plainBytes = Encoding.UTF8.GetBytes(plainTXT);
			byte[] encryptedBytes = rsaSrvs.Encrypt(plainBytes, false);

			return encryptedBytes;
		}
		public static string Decrypt(string privateKey, string EncBase64 = "", byte[] encryptedBytes = null)
		{
			if (string.IsNullOrEmpty(privateKey))
				throw new ArgumentNullException("privateKey");

			var rsaSrvs = new RSACryptoServiceProvider();


			if (!string.IsNullOrEmpty(EncBase64))
				encryptedBytes = Convert.FromBase64String(EncBase64);
			if (encryptedBytes == null || encryptedBytes.Length == 0)
				throw new ArgumentNullException("encryptedBytes");
			
			rsaSrvs.ImportCspBlob(Convert.FromBase64String(privateKey));

			byte[] plainBytes = rsaSrvs.Decrypt(encryptedBytes, false);

			string plainText = Encoding.UTF8.GetString(plainBytes, 0, plainBytes.Length);

			return plainText;
		}
	}
}
