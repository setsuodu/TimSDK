using System;
using System.Security.Cryptography;
using System.Text;

public static class CryptographyExtensions
{
	public enum HashType
	{
		Md5,
		Sha1,
		Sha256,
		Sha384,
		Sha512
	}

	/// <summary>
	/// 	Calculates the MD5 hash for the given string.
	/// </summary>
	/// <returns>A 32 char long hash.</returns>
	public static string GetHashMd5(this string input)
	{
		return ComputeHash(HashType.Md5, input);
	}

	/// <summary>
	/// 	Calculates the SHA-1 hash for the given string.
	/// </summary>
	/// <returns>A 40 char long hash.</returns>
	public static string GetHashSha1(this string input)
	{
		return ComputeHash(HashType.Sha1, input);
	}

	/// <summary>
	/// 	Calculates the SHA-256 hash for the given string.
	/// </summary>
	/// <returns>A 64 char long hash.</returns>
	public static string GetHashSha256(this string input)
	{
		return ComputeHash(HashType.Sha256, input);
	}

	/// <summary>
	/// 	Calculates the SHA-384 hash for the given string.
	/// </summary>
	/// <returns>A 96 char long hash.</returns>
	public static string GetHashSha384(this string input)
	{
		return ComputeHash(HashType.Sha384, input);
	}

	/// <summary>
	/// 	Calculates the SHA-512 hash for the given string.
	/// </summary>
	/// <returns>A 128 char long hash.</returns>
	public static string GetHashSha512(this string input)
	{
		return ComputeHash(HashType.Sha512, input);
	}

	public static string ComputeHash(HashType hashType, string input)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}

		var hasher = GetHasher(hashType);
		var inputBytes = Encoding.UTF8.GetBytes(input);

		var hashBytes = hasher.ComputeHash(inputBytes);
		var hash = new StringBuilder();
		foreach (var b in hashBytes)
		{
			hash.Append(string.Format("{0:x2}", b));
		}

		return hash.ToString();
	}

	private static HashAlgorithm GetHasher(HashType hashType)
	{
		switch (hashType)
		{
			case HashType.Md5:
				return new MD5CryptoServiceProvider();
			case HashType.Sha1:
				return new SHA1Managed();
			case HashType.Sha256:
				return new SHA256Managed();
			case HashType.Sha384:
				return new SHA384Managed();
			case HashType.Sha512:
				return new SHA512Managed();
			default:
				throw new ArgumentOutOfRangeException("hashType");
		}
	}
}
