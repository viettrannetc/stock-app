using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreSqlDb.Common
{
	public class Utilities
	{
		

		// Token: 0x0600003C RID: 60 RVA: 0x00002628 File Offset: 0x00000828
		public static byte[] CryptData(byte[] buffer, string password)
		{
			return Utilities.DES_Encrypt(buffer, password);
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00002631 File Offset: 0x00000831
		public static byte[] DecryptData(byte[] buffer, string password)
		{
			return Utilities.DES_Decrypt(buffer, password);
		}

		// Token: 0x0600003E RID: 62 RVA: 0x0000263C File Offset: 0x0000083C
		public static byte[] ObjectToBytes(object obj)
		{
			if (obj == null)
			{
				return null;
			}
			MemoryStream memoryStream = new MemoryStream();
			new BinaryFormatter().Serialize(memoryStream, obj);
			byte[] result = memoryStream.ToArray();
			memoryStream.Close();
			return result;
		}

		// Token: 0x0600003F RID: 63 RVA: 0x0000266C File Offset: 0x0000086C
		public static object BytesToObject(byte[] buffer)
		{
			MemoryStream memoryStream = new MemoryStream(buffer);
			object result = new BinaryFormatter().Deserialize(memoryStream);
			memoryStream.Close();
			buffer = null;
			GC.Collect();
			return result;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00002699 File Offset: 0x00000899
		public static string ByteArrayToString(byte[] pSourceByteArray)
		{
			return new ASCIIEncoding().GetString(pSourceByteArray).Replace("\0", string.Empty);
		}

		// Token: 0x06000041 RID: 65 RVA: 0x000026B8 File Offset: 0x000008B8
		public static void StringToByteArray(string pSourceString, ref byte[] pDesByteArray)
		{
			byte[] bytes = new ASCIIEncoding().GetBytes(pSourceString);
			for (int i = 0; i < bytes.Length; i++)
			{
				pDesByteArray[i] = bytes[i];
			}
		}

		// Token: 0x06000042 RID: 66 RVA: 0x000026E6 File Offset: 0x000008E6
		public static byte[] StringToByteArray(string pSourceString)
		{
			return new ASCIIEncoding().GetBytes(pSourceString);
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000026F4 File Offset: 0x000008F4
		public static string MD5(string cleanString)
		{
			byte[] bytes = new UnicodeEncoding().GetBytes(cleanString);
			return BitConverter.ToString(((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(bytes));
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00002728 File Offset: 0x00000928
		public static string SHA1(string cleanString)
		{
			byte[] bytes = new UnicodeEncoding().GetBytes(cleanString);
			return BitConverter.ToString(((HashAlgorithm)CryptoConfig.CreateFromName("SHA1")).ComputeHash(bytes));
		}

		//// Token: 0x06000045 RID: 69 RVA: 0x0000275C File Offset: 0x0000095C
		//public static string EncryptPassword(string cleanString, string publicKey)
		//{
		//	string result;
		//	try
		//	{
		//		publicKey = Utilities.FromBase64String(publicKey);
		//		result = Convert.ToBase64String(RSAKeys.RSAEncrypt(Encoding.UTF8.GetBytes(cleanString), publicKey));
		//	}
		//	catch
		//	{
		//		result = "";
		//	}
		//	return result;
		//}

		//// Token: 0x06000046 RID: 70 RVA: 0x000027A4 File Offset: 0x000009A4
		//public static string DecryptPassword(string cipherString, string privateKey)
		//{
		//	string result;
		//	try
		//	{
		//		byte[] dataToDecrypt = Convert.FromBase64String(cipherString);
		//		privateKey = Utilities.FromBase64String(privateKey);
		//		cipherString = Encoding.UTF8.GetString(RSAKeys.RSADecrypt(dataToDecrypt, privateKey));
		//		GC.Collect();
		//		result = cipherString;
		//	}
		//	catch
		//	{
		//		result = "";
		//	}
		//	return result;
		//}

		// Token: 0x06000047 RID: 71 RVA: 0x000027F8 File Offset: 0x000009F8
		public static string GetOS()
		{
			OperatingSystem osversion = Environment.OSVersion;
			string result = "Unknown";
			PlatformID platform = osversion.Platform;
			if (platform != PlatformID.Win32Windows)
			{
				if (platform == PlatformID.Win32NT)
				{
					switch (osversion.Version.Major)
					{
						case 3:
							result = "Windows NT 3.51";
							break;
						case 4:
							result = "Windows NT 4.0";
							break;
						case 5:
							if (osversion.Version.Minor == 0)
							{
								result = "Windows 2000";
							}
							else
							{
								result = "Windows XP";
							}
							break;
						case 6:
							result = "Windows Vista";
							break;
					}
				}
			}
			else
			{
				int minor = osversion.Version.Minor;
				if (minor != 0)
				{
					if (minor != 10)
					{
						if (minor == 90)
						{
							result = "Windows Me";
						}
					}
					else
					{
						result = "Windows 98";
					}
				}
				else
				{
					result = "Windows 95";
				}
			}
			return result;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x000028B2 File Offset: 0x00000AB2
		public static string ToBase64String(string data)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
		}

		// Token: 0x06000049 RID: 73 RVA: 0x000028C4 File Offset: 0x00000AC4
		public static string FromBase64String(string data)
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(data));
		}

		// Token: 0x0600004A RID: 74 RVA: 0x000028D8 File Offset: 0x00000AD8
		public static string DES_Encrypt(string data, string password)
		{
			string result;
			try
			{
				if (data == "")
				{
					result = data;
				}
				else
				{
					password = Utilities.MD5(password);
					byte[] array = Encoding.UTF8.GetBytes(data);
					int num = array.Length;
					DESCryptoServiceProvider descryptoServiceProvider = new DESCryptoServiceProvider();
					descryptoServiceProvider.Key = Encoding.ASCII.GetBytes(password.Substring(0, 8));
					descryptoServiceProvider.IV = Encoding.ASCII.GetBytes(password.Substring(8, 8));
					MemoryStream memoryStream = new MemoryStream();
					CryptoStream cryptoStream = new CryptoStream(memoryStream, descryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write);
					cryptoStream.Write(array, 0, array.Length);
					cryptoStream.FlushFinalBlock();
					array = memoryStream.ToArray();
					cryptoStream.Close();
					memoryStream.Close();
					result = BitConverter.ToString(array).Replace("-", "");
				}
			}
			catch
			{
				result = "";
			}
			return result;
		}

		// Token: 0x0600004B RID: 75 RVA: 0x000029B0 File Offset: 0x00000BB0
		private static byte[] FromBitConvertString(string data)
		{
			byte[] array = new byte[data.Length / 2];
			for (int i = 0; i < data.Length / 2; i++)
			{
				array[i] = Convert.ToByte(data.Substring(i * 2, 2), 16);
			}
			return array;
		}

		// Token: 0x0600004C RID: 76 RVA: 0x000029F4 File Offset: 0x00000BF4
		public static string DES_Decrypt(string data, string password)
		{
			string result;
			try
			{
				if (data == "")
				{
					result = data;
				}
				else
				{
					password = Utilities.MD5(password);
					byte[] array = Utilities.FromBitConvertString(data);
					DESCryptoServiceProvider descryptoServiceProvider = new DESCryptoServiceProvider();
					descryptoServiceProvider.Key = Encoding.ASCII.GetBytes(password.Substring(0, 8));
					descryptoServiceProvider.IV = Encoding.ASCII.GetBytes(password.Substring(8, 8));
					MemoryStream memoryStream = new MemoryStream();
					CryptoStream cryptoStream = new CryptoStream(memoryStream, descryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Write);
					cryptoStream.Write(array, 0, array.Length);
					cryptoStream.FlushFinalBlock();
					array = memoryStream.ToArray();
					cryptoStream.Close();
					memoryStream.Close();
					result = Encoding.UTF8.GetString(array);
				}
			}
			catch
			{
				result = "";
			}
			return result;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00002AB8 File Offset: 0x00000CB8
		public static byte[] DES_Encrypt(byte[] buffer, string password)
		{
			byte[] result;
			try
			{
				password = Utilities.MD5(password);
				int num = buffer.Length;
				DESCryptoServiceProvider descryptoServiceProvider = new DESCryptoServiceProvider();
				descryptoServiceProvider.Key = Encoding.ASCII.GetBytes(password.Substring(0, 8));
				descryptoServiceProvider.IV = Encoding.ASCII.GetBytes(password.Substring(8, 8));
				MemoryStream memoryStream = new MemoryStream();
				CryptoStream cryptoStream = new CryptoStream(memoryStream, descryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write);
				cryptoStream.Write(buffer, 0, buffer.Length);
				cryptoStream.FlushFinalBlock();
				buffer = memoryStream.ToArray();
				cryptoStream.Close();
				memoryStream.Close();
				result = buffer;
			}
			catch
			{
				result = new byte[1];
			}
			return result;
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00002B60 File Offset: 0x00000D60
		public static byte[] DES_Decrypt(byte[] buffer, string password)
		{
			byte[] result;
			try
			{
				password = Utilities.MD5(password);
				DESCryptoServiceProvider descryptoServiceProvider = new DESCryptoServiceProvider();
				descryptoServiceProvider.Key = Encoding.ASCII.GetBytes(password.Substring(0, 8));
				descryptoServiceProvider.IV = Encoding.ASCII.GetBytes(password.Substring(8, 8));
				MemoryStream memoryStream = new MemoryStream();
				CryptoStream cryptoStream = new CryptoStream(memoryStream, descryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Write);
				cryptoStream.Write(buffer, 0, buffer.Length);
				cryptoStream.FlushFinalBlock();
				buffer = memoryStream.ToArray();
				cryptoStream.Close();
				memoryStream.Close();
				result = buffer;
			}
			catch
			{
				result = new byte[1];
			}
			return result;
		}

		// Token: 0x0600004F RID: 79
		[DllImport("shell32.dll")]
		private static extern int SHFileOperation(ref Utilities.SHFILEOPSTRUCT lpFileOp);

		// Token: 0x06000050 RID: 80 RVA: 0x00002C04 File Offset: 0x00000E04
		public static void DeleteDirectory(string path)
		{
			path += "\0\0";
			Utilities.SHFILEOPSTRUCT shfileopstruct = default(Utilities.SHFILEOPSTRUCT);
			shfileopstruct.wFunc = 3;
			shfileopstruct.pFrom = path;
			shfileopstruct.fFlags = 4116;
			Utilities.SHFileOperation(ref shfileopstruct);
		}

		// Token: 0x04000010 RID: 16
		private const int FO_DELETE = 3;

		// Token: 0x04000011 RID: 17
		private const int FOF_ALLOWUNDO = 64;

		// Token: 0x04000012 RID: 18
		private const int FOF_NOCONFIRMATION = 16;

		// Token: 0x04000013 RID: 19
		private const int FOF_NOCONFIRMMKDIR = 512;

		// Token: 0x04000014 RID: 20
		private const int FOF_SIMPLEPROGRESS = 256;

		// Token: 0x04000015 RID: 21
		private const int FOF_SILENT = 4;

		// Token: 0x04000016 RID: 22
		private const int FOF_NORECURSION = 4096;

		// Token: 0x02000010 RID: 16
		private struct SHFILEOPSTRUCT
		{
			// Token: 0x0400001D RID: 29
			public int hwnd;

			// Token: 0x0400001E RID: 30
			public int wFunc;

			// Token: 0x0400001F RID: 31
			public string pFrom;

			// Token: 0x04000020 RID: 32
			public string pTo;

			// Token: 0x04000021 RID: 33
			public int fFlags;

			// Token: 0x04000022 RID: 34
			public bool fAnyOperationsAborted;

			// Token: 0x04000023 RID: 35
			public int hNameMappings;

			// Token: 0x04000024 RID: 36
			public int lpszProgressTitle;
		}
	}
}
