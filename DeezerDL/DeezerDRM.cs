using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace DeezerDL
{
    public class DeezerDRM
    {
         internal  byte[] DecryptChunk(byte[] buffer,int bytesRead, int currentBlock, string key)
            {
                byte[] bfkey = Encoding.UTF8.GetBytes(key);
                BlowfishEngine engine = new BlowfishEngine();
                BufferedBlockCipher cipher = new BufferedBlockCipher(new CbcBlockCipher(engine));
                KeyParameter keyParameters = ParameterUtilities.CreateKeyParameter("Blowfish", bfkey);
                ParametersWithIV param = new ParametersWithIV(keyParameters,new byte[] {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07});
                cipher.Init(false,param);
                bool isEncrypted = ((currentBlock % 3) == 0); // only every 3rd block of 2048 is encrypted 
                bool isWholeBlock = bytesRead == 2048;
                // Console.WriteLine($"is it encrypted? (3rd block) if not write straight away {isEncrypted}");
                // Console.WriteLine($"did we read 2048 bytes? if not write straight away {isWholeBlock}");
                // Console.WriteLine($"if this isnt 2048 then something really fucked up {buffer.Length}");
                // Console.WriteLine($"the amount of bytes deezer gave us in this chunk {bytesRead}");
            


                if (isEncrypted & isWholeBlock)
                {
                    // WHY IS THERE NO BLOWFISH FUNC MICROFUCK
                    byte[] decryptedData = new byte[cipher.GetOutputSize(bytesRead)];
                    // Console.WriteLine($"preDecrypted Chunk: {BitConverter.ToString(decryptedData)}");
                    int len = cipher.ProcessBytes(buffer, 0, bytesRead, decryptedData, 0);
                    len += cipher.DoFinal(decryptedData, len);
                    // Resize the output array to the actual size
                    Array.Resize(ref decryptedData, len);
                    // Console.WriteLine($"Decrypted Chunk: {BitConverter.ToString(decryptedData)}");
                    return decryptedData;
                }
                else
                {
                    // Copy the unencrypted bytes directly
                    byte[] unencryptedData = new byte[bytesRead];
                    Array.Copy(buffer, 0, unencryptedData, 0, bytesRead);
                    // Console.WriteLine($"uDecrypted Chunk: {BitConverter.ToString(unencryptedData)}");
                    return unencryptedData;
                    
                }
            }
         
        // private static byte[] AddPadding(byte[] data, int blockSize)
        // {
        //     int paddingLength = blockSize - (data.Length % blockSize);
        //     if (paddingLength == 0)
        //         return data;
        //
        //     byte[] paddedData = new byte[data.Length + paddingLength];
        //     Buffer.BlockCopy(data, 0, paddedData, 0, data.Length);
        //
        //     // PKCS#7 padding
        //     for (int i = data.Length; i < paddedData.Length; i++)
        //     {
        //         paddedData[i] = (byte)paddingLength;
        //     }
        //     return paddedData;
        // }
        //
        // private static byte[] RemovePadding(byte[] paddedData)
        // {
        //     int paddingLength = paddedData[paddedData.Length - 1];
        //     if (paddingLength is > 0 and <= 16)
        //     {
        //         return paddedData.Take(paddedData.Length - paddingLength).ToArray();
        //     }
        //     return paddedData;
        // }
        internal  string CalcBlowfish(uint songId)
        {
            MD5 md5 = MD5.Create();
            byte[] bfKey = {0x67, 0x34, 0x65, 0x6c, 0x35, 0x38, 0x77, 0x63, 0x30, 0x7a, 0x76, 0x66, 0x39, 0x6e, 0x61, 0x31 };
            byte[] songIdBytes = Encoding.UTF8.GetBytes(songId.ToString());
            var songIdMd5 = BitConverter.ToString(md5.ComputeHash(songIdBytes)).Replace("-", "").ToLower();
            StringBuilder decryptionKey = new StringBuilder();
            for (int i = 0; i < 16; i++)
            {
                char dChar = Convert.ToChar((songIdMd5[i] ^ songIdMd5[i + 16] ^ bfKey[i]));
                decryptionKey = decryptionKey.Append(dChar);
            }
            md5.Dispose();
            return decryptionKey.ToString();
        }

        private byte[] GenAesCrypt(byte[] data, byte[] key)
        {
            using Aes myAes = Aes.Create();
            myAes.Mode = CipherMode.ECB;
            myAes.Key = key;
            myAes.Padding = PaddingMode.None;
            
            var encryptor = myAes.CreateEncryptor(myAes.Key, myAes.IV);
            var encryptedBytes = encryptor.TransformFinalBlock(data, 0, data.Length);
            return encryptedBytes;
        }

        // generates the data needed for a direct download
        internal string GenUrLdata(uint trackId, string trackHash, int mediaVer, int fmt)
        {
            
            MD5 md5 = MD5.Create();
            byte[] md5OriginBytes = Encoding.UTF8.GetBytes(trackHash);
            byte[] fmtBytes = Encoding.UTF8.GetBytes(fmt.ToString());
            byte[] songIdBytes = Encoding.UTF8.GetBytes(trackId.ToString());
            byte[] mediaVerBytes = Encoding.UTF8.GetBytes(mediaVer.ToString());
            byte[] a4Byte = { 0xa4 };
            List<byte[]> concatByteArrayList = new List<byte[]>
            {
                md5OriginBytes,
                fmtBytes,
                songIdBytes,
                mediaVerBytes
            };
            var dataConcat = ArrayJoin(a4Byte[0],concatByteArrayList);
            string md5Hex = BitConverter.ToString(md5.ComputeHash(dataConcat)).Replace("-", "").ToLower();
            byte[] md5Bytes = Encoding.UTF8.GetBytes(md5Hex);
            List<byte[]> byteArrayList = new List<byte[]>
            {
                md5Bytes,
                dataConcat
            };
            var data = ArrayJoin(a4Byte[0], byteArrayList);
            byte[] append = new byte[data.Length + 1];
            Buffer.BlockCopy(data, 0, append, 0, data.Length);
            append[append.Length - 1] = a4Byte[0];
            int blockSize = 16;
            int length = append.Length;
    
            // Calculate the amount of padding required
            int paddingNeeded = (blockSize - (length % blockSize)) % blockSize;
    
            // Create a new array with the padded length
            byte[] paddedData = new byte[length + paddingNeeded];
    
            // Copy the original data into the new array
            Buffer.BlockCopy(append, 0, paddedData, 0, length);
            md5.Dispose();
            byte[] encrypt = GenAesCrypt(paddedData,Encoding.UTF8.GetBytes("jo6aey6haid2Teih"));
            return $"https://e-cdns-proxy-{trackHash[0]}.dzcdn.net/mobile/1/{ByteArrayToHexString(encrypt)}";
        }

        private static byte[] ArrayJoin(byte separator, List<byte[]> arrays)
        {
            using MemoryStream result = new MemoryStream();
            byte[] first = arrays.First();
            result.Write(first, 0, first.Length);
    
            foreach (var array in arrays.Skip(1))
            {
                result.WriteByte(separator);
                result.Write(array, 0, array.Length);
            }

            return result.ToArray();
        }
        private static string ByteArrayToHexString(byte[] byteArray)
        {
            // Convert the byte array to a hexadecimal string with dashes
            string hexString = BitConverter.ToString(byteArray);

            // Remove dashes
            hexString = hexString.Replace("-", "");

            return hexString;
        }
    }
}