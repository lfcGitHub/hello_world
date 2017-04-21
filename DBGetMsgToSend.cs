using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using System.Xml;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using CommonDefine;

namespace ConsoleApplication1
{
	class DBGetMsgToSend
	{
		Socket clientSocket;
		SqlConnection clientsqlconnection = new SqlConnection(ConfigurationManager.AppSettings["SqlConnectionStr"]);
        DBWriteMsg m_dbTimeOutWrite = new DBWriteMsg();
		private string Simcard_NO;
		private byte m_ver = 0;

		public string _simcard_no
		{
			set { Simcard_NO = value; }
		}

		public byte _m_ver
		{
			set { m_ver = value; }
		}
		public DBGetMsgToSend(/*Socket _socket*/)
		{
			/*clientSocket = _socket*/;
		}
		public Socket _cliengSocket
		{
			set { clientSocket = value;}
		}

        public void CheckTimeOutCommand(bool isRecTimeout, string Simcard_NO)
        {
            try
            {
                clientsqlconnection.Open();
                SqlCommand cmd2 = new SqlCommand("IsTimeOutCommand", clientsqlconnection);
                cmd2.CommandType = CommandType.StoredProcedure;
                cmd2.Parameters.AddWithValue("@License", Simcard_NO);
                SqlDataAdapter sda = new SqlDataAdapter(cmd2);
                DataSet ds = new DataSet();
                sda.Fill(ds);
                int rowcount = ds.Tables[0].Rows.Count;
                string[] strMsgDateTime = new string[rowcount];
                string[] strMsgID = new string[rowcount];
                string[] strMsgDowninfo = new string[rowcount];

                for (int i = 0; i < rowcount; i++)
                {
                    strMsgID[i] = ds.Tables[0].Rows[i][0].ToString();
                    strMsgDateTime[i] = ds.Tables[0].Rows[i][4].ToString();
                    strMsgDowninfo[i] = ds.Tables[0].Rows[i][2].ToString();
                }

                for (int i = 0; i < rowcount; i++)
                {
                    System.DateTime currentTime = new System.DateTime();
                    currentTime = System.DateTime.Now;
                    string strcurrentTime = System.DateTime.Now.ToString();
                    DateTime curtime = Convert.ToDateTime(strcurrentTime);
                    DateTime datetime = Convert.ToDateTime(strMsgDateTime[i]);
                    TimeSpan cur = new TimeSpan(curtime.Ticks);
                    TimeSpan dat = new TimeSpan(datetime.Ticks);
                    TimeSpan diff = cur.Subtract(dat).Duration();
                    Console.Write("CheckTimeOutCommand,{0}\n", diff.TotalSeconds);

                    if (isRecTimeout)
                    {
                        string strbigcmdandsmallcmd = strMsgDowninfo[i].Substring(0, 4);
                        byte[] type = Encoding.ASCII.GetBytes(strbigcmdandsmallcmd);
                        byte[] bigandsmall = new byte[2];
                        byte temp;
                        for (int k = 0; k < 2; k++)
                        {
                            temp = type[2 * k];
                            if (temp >= '0' && temp <= '9')
                            {
                                temp -= 48;
                            }
                            else
                            {
                                temp -= 87;
                            }
                            bigandsmall[k] = (byte)(temp << 4);
                            temp = type[2 * k + 1];
                            if (temp >= '0' && temp <= '9')
                            {
                                temp -= 48;
                            }
                            else
                            {
                                temp -= 87;
                            }
                            bigandsmall[k] += (byte)temp;
                        }
                        byte bigcmd = bigandsmall[0];
                        byte smallcmd = bigandsmall[1];
                        try
                        {
                            m_dbTimeOutWrite.DownLoadType(bigcmd, smallcmd, Simcard_NO,clientsqlconnection);
                            m_dbTimeOutWrite.InsertDoAnswer(1, true, clientsqlconnection, int.Parse(strMsgID[i]), Simcard_NO);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("m_dbTimeOutWrite.InsertDoAnswer出错!");
                        }
                    }
                    else
                    {
                        if (diff.TotalSeconds > 180)
                        {
                            string strbigcmdandsmallcmd = strMsgDowninfo[i].Substring(0, 4);
                            byte[] type = Encoding.ASCII.GetBytes(strbigcmdandsmallcmd);
                            byte[] bigandsmall = new byte[2];
                            byte temp;
                            for (int k = 0; k < 2; k++)
                            {
                                temp = type[2 * k];
                                if (temp >= '0' && temp <= '9')
                                {
                                    temp -= 48;
                                }
                                else
                                {
                                    temp -= 87;
                                }
                                bigandsmall[k] = (byte)(temp << 4);
                                temp = type[2 * k + 1];
                                if (temp >= '0' && temp <= '9')
                                {
                                    temp -= 48;
                                }
                                else
                                {
                                    temp -= 87;
                                }
                                bigandsmall[k] += (byte)temp;
                            }
                            byte bigcmd = bigandsmall[0];
                            byte smallcmd = bigandsmall[1];
                            try
                            {
                                m_dbTimeOutWrite.DownLoadType(bigcmd, smallcmd, Simcard_NO,clientsqlconnection);
                                m_dbTimeOutWrite.InsertDoAnswer(1, true, clientsqlconnection, int.Parse(strMsgID[i]), Simcard_NO);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("m_dbTimeOutWrite.InsertDoAnswer出错!");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Simcard_NO + "CheckTimeOutCommand: " + ex.Message + ex.Source);
            }
            finally
            {
                clientsqlconnection.Close();
            }
        }

		public byte[] GetCommand()
		{
			bool bGetMsg = false;
			SqlDataReader myReader = null;

			string strMsgData = "";
			string strMsgID = "";
			byte[] byMsg = null;
			byte[] byCmd = null;
			try
			{
				clientsqlconnection.Open();
				#region AdMsg_BySim
				try
				{
					SqlCommand cmd2 = new SqlCommand("IssuedInstructionState", clientsqlconnection);
					cmd2.CommandType = CommandType.StoredProcedure;
					cmd2.Parameters.AddWithValue("@License", Simcard_NO);
					myReader = cmd2.ExecuteReader();
					if (myReader.Read())
					{
						strMsgID = myReader["ID"].ToString();
						strMsgData = myReader["Instruction"].ToString();

//                        Console.WriteLine("从数据库读取的 strMsgID : {0}, strMsgData :{1}\n", strMsgID, strMsgData);
						bGetMsg = true;
					}

					myReader.Close();
					myReader = null;
					Console.WriteLine(Simcard_NO + ": " + ex.Message + ex.Source);					
				}
				catch (Exception ex)
				{
                    Console.WriteLine("无命令读取，或者读取命令不成功!\n");
					Console.WriteLine(Simcard_NO + ": " + ex.Message + ex.Source);
                    return null;
				}
				#endregion
				do
				{
                    if (!bGetMsg)
                    {
//                        Console.WriteLine("无命令!\n");
                        return null;
                    }

					if (strMsgData.Length % 2 != 0)
					{
                        Console.WriteLine("strMsgData.Length" + "% 2 != 0");
                        return null;
					}
					try
					{
//                        Console.WriteLine("Hex2ByteArr" + Simcard_NO + strMsgData);
                        strMsgData = strMsgData.Trim();
						byMsg = CommonDefine.MyCommonFunc.Hex2ByteArr(strMsgData);
//                        Console.WriteLine("iSerial : {0}", byMsg.Length);
					}
					catch (System.Exception ex)
					{
						Console.WriteLine("Hex2ByteArr error" + Simcard_NO  + strMsgData + ": "+ ex.Message + ex.Source);
                        return null;
					}
				} while (false);

				byCmd = new byte[byMsg.Length + 14];
				byCmd[0] = (byte)'U';
				byCmd[1] = (byte)'C';
				byCmd[2] = BitConverter.GetBytes(/*IPAddress.NetworkToHostOrder*/(byCmd.Length))[0];
				byCmd[3] = BitConverter.GetBytes(/*IPAddress.NetworkToHostOrder*/(byCmd.Length))[1];
				byCmd[4] = BitConverter.GetBytes(/*IPAddress.NetworkToHostOrder*/(byCmd.Length))[2];
				byCmd[5] = BitConverter.GetBytes(/*IPAddress.NetworkToHostOrder*/(byCmd.Length))[3];
				byCmd[6] = m_ver;

				int iSerial = Convert.ToInt32(strMsgID);
//                Console.WriteLine("iSerial : {0}", iSerial);
                byCmd[7] = BitConverter.GetBytes(/*IPAddress.NetworkToHostOrder*/(iSerial))[0];
                byCmd[8] = BitConverter.GetBytes(/*IPAddress.NetworkToHostOrder*/(iSerial))[1];
                byCmd[9] = BitConverter.GetBytes(/*IPAddress.NetworkToHostOrder*/(iSerial))[2];
                byCmd[10] = BitConverter.GetBytes(/*IPAddress.NetworkToHostOrder*/(iSerial))[3];
				System.Array.Copy(byMsg, 0, byCmd, 11, byMsg.Length);

				uint isum = 0;
				for (int i = 2; i < byCmd.Length - 3; i++)
				{
					isum += (uint)byCmd[i];
				}
// 				byCmd[11 + byMsg.Length] = (byte)(isum % 256);
// 				byCmd[11 + byMsg.Length] = (byte)'\r';
// 				byCmd[11 + byMsg.Length] = (byte)'\n';

//                 uint isum = 0;
//                 for (int i = 0; i <= 12; i++)
//                 {
//                 	isum += (uint)byCmd[i];
//                 }

                byCmd[byCmd.Length - 3] = (byte)(isum % 256);
                byCmd[byCmd.Length - 2] = (byte)'\r';
                byCmd[byCmd.Length - 1] = (byte)'\n';
			}
			finally
			{
				if (myReader != null) myReader.Close();
				clientsqlconnection.Close();				
			}
			if (!bGetMsg)
			{
 //               Console.WriteLine("return" + "bGetMsg" + "false" + "2");
				return null;
			}
            return byCmd;
		}
	}
}
