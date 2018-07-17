using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace myNetSend
{
    static class NetSend
    {
        #region constants

        private static byte[] NBSSsessionRequestHeader = new byte[] { 0x81, 0x00 };
        private static byte[] NBSSsessionSuccessResponseHeader = new byte[] { 0x82, 0x00, 0x00, 0x00 };

        private static byte[] smbHeaderSingleMessage = new byte[] {     0xff, 0x53, 0x4d, 0x42, 0xd0, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00 };

        private static byte[] smbHeaderStartMultiMessage = new byte[] {  0xff, 0x53, 0x4d, 0x42, 0xd5, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00 };

        private static byte[] smbHeaderRegularMultiMessage = new byte[] {  0xff, 0x53, 0x4d, 0x42, 0xd7, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00 };

        private static byte[] smbSuccessReceiveSingleMessage = new byte[]
        {
                                                                        0x00, 0x00, 0x00, 0x23, 0xff, 0x53,
                                                                        0x4d, 0x42, 0xd0, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00
        };
        /// <summary>
        /// 3,4,5 байты с конца должны быть скопированы в ответ
        /// </summary>
        private static byte[] smbSuccessReceiveStartMultiMessage = new byte[]
        {
                                                                        0x00, 0x00, 0x00, 0x25, 0xff, 0x53,
                                                                        0x4d, 0x42, 0xd5, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x01, 0x03, 0x00, 0x00, 0x00
        };
        private static byte[] smbSuccessReceiveRegularMultiMessage = new byte[]
        {
                                                                        0x00, 0x00, 0x00, 0x23, 0xff, 0x53,
                                                                        0x4d, 0x42, 0xd7, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00
        };
        private static byte[] smbSuccessReceiveEndMultiMessage = new byte[]
        {
                                                                        0x00, 0x00, 0x00, 0x23, 0xff, 0x53,
                                                                        0x4d, 0x42, 0xd6, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00
        };

        private static byte[] smbSendEndMultiMessage = new byte[]
        {
                                                                        0x00, 0x00, 0x00, 0x25, 0xff, 0x53,
                                                                        0x4d, 0x42, 0xd6, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                                        0x00, 0x00, 0x00, 0x00, 0x00
        };

        #endregion

        public static void SendMessage(string dstName, string srcName, string msg)
        {
            IPHostEntry hostEntry;
            hostEntry = Dns.GetHostEntry(dstName);

            if (hostEntry.AddressList.Length > 0)
            {
                var ip = hostEntry.AddressList[0];
                SendMessage(dstName, ip.ToString(), srcName, msg);
            }
            else
            {
                throw new Exception("Не удалось узнать IP из имени");
            }
        }

        public static void SendMessage(string dstName, string dstIP, string srcName, string msg)
        {
            if (msg.Length == 0)
            {
                throw new Exception("Вы не указали текст сообщения");
            }

            try
            {
                using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    List<byte> packet = new List<byte>();
                    packet.AddRange(NBSSsessionRequestHeader);
                    packet.Add(0x00);
                    byte[] machineNameDst = Convert_cp866_HostName_To_SMB_HostName(dstName.ToUpper());
                    byte[] machineNameSrc = Convert_cp866_HostName_To_SMB_HostName(srcName.ToUpper());
                    packet.Add((byte)(machineNameDst.Length + machineNameSrc.Length));
                    packet.AddRange(machineNameDst);
                    packet.AddRange(machineNameSrc);

                    sock.Connect(dstIP, 139);
                    sock.SendTimeout = 2 * 1000; //2 сек таймаут
                    sock.Send(packet.ToArray());

                    byte[] receiveBuf = new byte[4];
                    sock.Receive(receiveBuf);
                    if (!receiveBuf.SequenceEqual(NBSSsessionSuccessResponseHeader))
                    {
                        sock.Close();
                        throw new Exception("Рецепиент не захотел устанавливать NBSS сессию.");
                    }

                    //----успешно подключились, теперь можно слать само сообщение----

                    if (msg.Length <= 128)
                    {//одиночное сообщение
                        packet.Clear();
                        packet.AddRange(PrepareSingleMsgPacket(srcName.ToUpper(), dstName.ToUpper(), msg));
                        sock.Send(packet.ToArray());

                        receiveBuf = new byte[smbSuccessReceiveSingleMessage.Length];
                        sock.Receive(receiveBuf);
                        if (!receiveBuf.SequenceEqual(smbSuccessReceiveSingleMessage))
                        {
                            sock.Close();
                            throw new Exception("Рецепиент не прислал подтверждение получения smb сообщения.");
                        }
                    }
                    else
                    {//многострочное сообщение
                        packet.Clear();
                        packet.AddRange(PrepareStartMultiMsgPacket(srcName.ToUpper(), dstName.ToUpper()));

                        sock.Send(packet.ToArray());
                        receiveBuf = new byte[packet.Count];
                        sock.Receive(receiveBuf);

                        if (!(receiveBuf[4] == 0xff && receiveBuf[5] == 0x53 && receiveBuf[6] == 0x4d &&
                             receiveBuf[7] == 0x42 && receiveBuf[8] == 0xd5 && receiveBuf[9] == 0x00))
                        {
                            sock.Close();
                            throw new Exception("Рецепиент не прислал подтверждение получения smb сообщения.");
                        }

                        byte[] tmp = new byte[3];
                        tmp[0] = receiveBuf[36];
                        tmp[1] = receiveBuf[37];
                        tmp[2] = receiveBuf[38];

                        int i = 0;
                        while (i < msg.Length)
                        {
                            packet.Clear();
                            if (i + 128 < msg.Length)
                            {
                                packet.AddRange(PrepareRegularMultiMsgPacket(srcName.ToUpper(), dstName.ToUpper(), tmp, msg.Substring(i, 128)));
                            }
                            else
                            {
                                packet.AddRange(PrepareRegularMultiMsgPacket(srcName.ToUpper(), dstName.ToUpper(), tmp, msg.Substring(i, msg.Length - i)));
                            }

                            sock.Send(packet.ToArray());
                            receiveBuf = new byte[smbSuccessReceiveRegularMultiMessage.Length];
                            sock.Receive(receiveBuf);
                            if (!receiveBuf.SequenceEqual(smbSuccessReceiveRegularMultiMessage))
                            {
                                sock.Close();
                                throw new Exception("Рецепиент не прислал подтверждение получения smb сообщения.");
                            }
                            i += 128;
                        }
                        packet.Clear();
                        packet.AddRange(PrepareEndMultiMsgPacket(srcName.ToUpper(), dstName.ToUpper(), tmp));
                        sock.Send(packet.ToArray());
                        receiveBuf = new byte[smbSuccessReceiveEndMultiMessage.Length];
                        sock.Receive(receiveBuf);
                        if (!receiveBuf.SequenceEqual(smbSuccessReceiveEndMultiMessage))
                        {
                            sock.Close();
                            throw new Exception("Рецепиент не прислал подтверждение получения smb сообщения.");
                        }
                    }
                    sock.Close();
                }
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message);
            }
        }

        private static byte[] PrepareRegularMultiMsgPacket(string srcAddr, string dstAddr, byte[] currMsgID, string msg)
        {
            //byte[] netBiosLength = new byte[3];
            byte[] msgBytes = Encoding.GetEncoding("cp866").GetBytes(msg);
            List<byte> body = new List<byte>();

            body.AddRange(smbHeaderRegularMultiMessage);
            body.AddRange(currMsgID);

            List<byte> tmp = new List<byte>();
            tmp.Add(0x00);
            tmp.Add(0x01);
            tmp.Add((byte)(msg.Length));
            tmp.Add(0x00);
            tmp.AddRange(msgBytes);

            body.Add((byte)(tmp.Count - 1));
            body.AddRange(tmp);

            tmp.Clear();
            tmp.Add(0x00);
            tmp.Add(0x00);
            tmp.Add(0x00);
            tmp.Add((byte)body.Count);
            tmp.AddRange(body);

            return tmp.ToArray();
        }

        private static byte[] PrepareStartMultiMsgPacket(string srcAddr, string dstAddr)
        {
            //byte[] netBiosLength = new byte[3];
            List<byte> body = new List<byte>();

            body.AddRange(smbHeaderStartMultiMessage);
            body.Add(0x00);

            List<byte> tmp = new List<byte>();
            tmp.Add(0x04);
            byte[] src = Encoding.GetEncoding("cp866").GetBytes(srcAddr);
            byte[] dst = Encoding.GetEncoding("cp866").GetBytes(dstAddr);
            tmp.AddRange(src);
            tmp.Add(0x00);
            tmp.Add(0x04);
            tmp.AddRange(dst);
            tmp.Add(0x00);

            body.Add((byte)tmp.Count);
            body.Add(0x00);
            body.AddRange(tmp.ToArray());
            tmp.Clear();
            tmp.Add(0x00);
            tmp.Add(0x00);
            tmp.Add(0x00);
            tmp.Add((byte)body.Count);
            tmp.AddRange(body.ToArray());
            return tmp.ToArray();
        }

        private static byte[] PrepareEndMultiMsgPacket(string srcAddr, string dstAddr, byte[] currMsgID)
        {
            List<byte> body = new List<byte>();
            body.AddRange(smbSendEndMultiMessage);
            body[36] = currMsgID[0];
            body[37] = currMsgID[1];
            body[38] = currMsgID[2];

            return body.ToArray();
        }

        private static byte[] PrepareSingleMsgPacket(string srcAddr, string dstAddr, string msg)
        {
            //byte[] netBiosLength = new byte[3];
            List<byte> body = new List<byte>();

            body.AddRange(smbHeaderSingleMessage);
            body.Add(0x00);

            List<byte> tmp = new List<byte>();
            tmp.Add(0x04);
            byte[] src = Encoding.GetEncoding("cp866").GetBytes(srcAddr);
            byte[] dst = Encoding.GetEncoding("cp866").GetBytes(dstAddr);
            tmp.AddRange(src);
            tmp.Add(0x00);
            tmp.Add(0x04);
            tmp.AddRange(dst);
            tmp.Add(0x00);
            tmp.Add(0x01);
            tmp.Add((byte)msg.Length);
            tmp.Add(0x00);
            tmp.AddRange(Encoding.GetEncoding("cp866").GetBytes(msg));

            body.Add((byte)tmp.Count);
            body.Add(0x00);
            body.AddRange(tmp.ToArray());
            tmp.Clear();
            tmp.Add(0x00);
            tmp.Add(0x00);
            tmp.Add(0x00);
            tmp.Add((byte)body.Count);
            tmp.AddRange(body.ToArray());
            return tmp.ToArray();
        }

        private static byte[] Convert_cp866_HostName_To_SMB_HostName(string inputName)
        {
            Encoding enc_ascii = Encoding.ASCII; ;
            Encoding enc_cp866 = Encoding.GetEncoding("cp866");
            byte[] b_ascii = enc_ascii.GetBytes(inputName);
            byte[] b_cp866 = Encoding.Convert(enc_ascii, enc_cp866, b_ascii);
            string inptName = enc_cp866.GetString(b_cp866);

            List<byte> compressed = new List<byte>();
            compressed.Add(0x20);
            for (int i = 0; i < inptName.Length; i++)
            {
                compressed.Add(Convert.ToByte(Convert.ToByte(Convert.ToByte(inptName[i]) >> 4) + Convert.ToByte('A')));
                compressed.Add(Convert.ToByte(Convert.ToByte(Convert.ToByte(inptName[i] & 0xF) + Convert.ToByte('A'))));

            }

            while (compressed.Count < 30)
            {
                compressed.Add(Convert.ToByte(Convert.ToByte(Convert.ToByte(' ') >> 4) + Convert.ToByte('A')));
                compressed.Add(Convert.ToByte(Convert.ToByte(Convert.ToByte(' ' & 0xF) + Convert.ToByte('A'))));
            }
            compressed.Add(0x41);
            compressed.Add(0x44);
            compressed.Add(0x00);

            return compressed.ToArray();
        }
    }
}
