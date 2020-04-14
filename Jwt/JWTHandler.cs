using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace JWTManager
{
	public class JWTHandler
	{

		private NameValueCollection JWTSection { get { return ConfigurationManager.GetSection("JWTSecurity") as NameValueCollection; } }
		/// <summary>
		/// Secret code, from the constractor
		/// </summary>
		private string _secrt;
		private string secret
		{
			get
			{
				if (string.IsNullOrEmpty(_secrt)) //ToDo:encrypt the key in Web config,
					return JWTSection["JWTSecret"].ToString();
				return _secrt;

			}
			set
			{
				_secrt = value;
			}
		}

		/// <summary>
		/// Issuer "Site name", from the constractor
		/// </summary>
		private string _issuer;
		private string issuer
		{
			get
			{
				if (string.IsNullOrEmpty(_issuer) && JWTSection != null) //ToDo:encrypt the key in Web config,
					return JWTSection["JWTIssuer"].ToString();
				return _issuer;
			}
			set
			{
				_issuer = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private int _tokenLifespan;
		private int tokenLifespan
		{
			get
			{
				if (JWTSection != null)
					if (JWTSection["JWTtokenLifespan"] != null)
						return int.Parse(JWTSection["JWTtokenLifespan"].ToString());
				if (_tokenLifespan <= 0)
					return 60;
				return _tokenLifespan;
			}
			set
			{
				_tokenLifespan = value;
			}
		}

		/// <summary>
		/// server certificate name
		/// </summary>
		private string _certName;
		public string certName
		{
			get
			{
				if (JWTSection != null)
					if (JWTSection["certLocation"] != null)
						return JWTSection["certLocation"].ToString();
				return _certName;
			}
			set
			{
				_certName = value;
			}
		}


		public JWTHandler()
		{ }

		/// <summary>
		/// To set the handler configuration
		/// </summary>
		/// <param name="Secret">The secret key, send null or empty to get it from the config file.</param>
		/// <param name="Issuer">The issue should be the site name, send null or empty to get it from the config file.</param>
		/// <param name="TokenLifespan">The lifespan for the claims, send 0 to get it from the config file.</param>
		/// <remarks>send 0 or not adding the tokenlifespan key in web config will make it 60 minute by default</remarks>
		public JWTHandler(string Secret, string Issuer, int TokenLifespan)
		{
			secret = Secret;
			issuer = Issuer;
			tokenLifespan = TokenLifespan;
		}

		/// <summary>
		/// Creating a new token with subject as a claim with the key you will provide
		/// </summary>
		/// <param name="TokenKey">The token subject</param>
		/// <returns><typeparamref name="JwtSecurityToken">token</typeparamref></returns>
		public string IssueToken(string TokenKey)
		{
			byte[] key = Encoding.UTF8.GetBytes(secret);
			SymmetricSecurityKey securityKey = new SymmetricSecurityKey(key);

			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
			SecurityTokenDescriptor stDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, TokenKey) }),
				Expires = DateTime.UtcNow.AddMinutes(tokenLifespan),
				Issuer = issuer,
				SigningCredentials = credentials
				//SigningCredentials = new X509SigningCredentials(GetCert(StoreName.Root,StoreLocation.LocalMachine,certName))
				
			};

			JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
			JwtSecurityToken token = handler.CreateJwtSecurityToken(stDescriptor);

			return handler.WriteToken(token);
		}

		/// <summary>
		/// To validate the request token
		/// </summary>
		/// <param name="token">The request token</param>
		/// <returns>The key that used when generate the token</returns>
		public string ValidateToken(string token)
		{
			ClaimsPrincipal principal = GetPrincipal(token);
			if (principal == null)
				return null;
			ClaimsIdentity identity = null;
			try
			{
				identity = (ClaimsIdentity)principal.Identity;

			}
			catch (NullReferenceException)
			{
				return null;
			}
			Claim tokenKeyClaim = identity.FindFirst(ClaimTypes.Name);
			string TokenKey = tokenKeyClaim.Value;
			return TokenKey;
		}

		/// <summary>
		/// Get the principal from the token
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		private ClaimsPrincipal GetPrincipal(string token)
		{

			try
			{
				JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
				JwtSecurityToken sToken = (JwtSecurityToken)handler.ReadToken(token);
				if (sToken == null)
					return null;
				byte[] key = Encoding.UTF8.GetBytes(secret);
				TokenValidationParameters parameters = new TokenValidationParameters()
				{
					RequireExpirationTime = true,
					ValidateIssuerSigningKey = true,
					ValidateIssuer = true,
					ValidIssuer = issuer,
					ValidateAudience = false,
					IssuerSigningKey = new SymmetricSecurityKey(key)
					//IssuerSigningKey = new X509SecurityKey(GetCert(StoreName.Root, StoreLocation.LocalMachine, certName))
				};
				SecurityToken securityToken;
				ClaimsPrincipal principal = handler.ValidateToken(token, parameters, out securityToken);
				return principal;
			}
			catch
			{
				return null;
			}
		}

		private X509Certificate2 GetCert(StoreName store, StoreLocation location, string certName)
		{

			X509Certificate2 certSelected = null;
			X509Store x509Store = new X509Store(store, location);
			x509Store.Open(OpenFlags.ReadOnly);

			X509Certificate2Collection certCol = x509Store.Certificates;
			foreach (X509Certificate2 c in certCol)
			{
				if (c.Subject == certName)
				{
					certSelected = c;
				}
			}
			x509Store.Close();

			return certSelected;
		}


	}
}
