using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        SerialPort serialPort1 = new SerialPort();
       
        public Form1()
        {
            InitializeComponent();
        }



        bool Sync_After = false;
        byte Packet_TX_Index = 0;
        byte Data_Prev = 0; // 직전값.
        byte PUD0 = 0;
        byte CRD_PUD2_PCDT = 0;
        byte PUD1 = 0;
        byte PacketCount = 0;
        byte PacketCyclicData = 0;
        byte psd_idx = 0;
        String PacketStreamData;
        int Ch_Num = 1;
        int Sample_Num = 1; 


        int Parsing_LXSDFT2(byte data_crnt)
        {
            int retv = 0;
            if (Data_Prev == 255 && data_crnt == 254)// 싱크지점 찾았다.
            {
                Sync_After = true;
                Packet_TX_Index = 0; // 패킷 TX인덱스 0으로 초기화.
            }

            Data_Prev = data_crnt; // 현재 값을 직전 값으로 받아둔다.
            if (Sync_After == true) // 싱크가 발견된 이후에만 실행된다.
            {
                Packet_TX_Index++; // TX인덱스 1증가. 254가 발견된 지점이 1이다. 시리얼로 1바이트 수신될때마다 1씩 증가하는것.
                if (Packet_TX_Index > 1) // TX인덱스 2이상만 수행된다.
                {
                    if (Packet_TX_Index == 2) // TX인덱스2 PUD0 확보.
                        PUD0 = data_crnt;
                    else if (Packet_TX_Index == 3) // TX인덱스3 CRD, PUD2, PCD Type 확보.
                        CRD_PUD2_PCDT = data_crnt;
                    else if (Packet_TX_Index == 4) // TX인덱스4 PC 확보. 
                        PacketCount = data_crnt;
                    else if (Packet_TX_Index == 5) // TX인덱스5 PUD1 확보.
                        PUD1 = data_crnt;
                    else if (Packet_TX_Index == 6) // TX인덱스6 PCD(패킷순환데이터) 확보.
                        PacketCyclicData = data_crnt;
                    else if (Packet_TX_Index > 6) // TX인덱스 7이상에는 스트림데이터(파형 데이터) 1바이트씩 순차적으로 들어온다. 데이터 수신되는 순서 -> 

                    {
                        psd_idx = (byte)(Packet_TX_Index - 7); // PacketStreamData배열의 인덱스.
                        //PacketStreamData[psd_idx] = data_crnt; // crnt_data를 순차적으로 확보하여 스트림데이터만 확보한다.
                        if (Packet_TX_Index == (Ch_Num * 2 * Sample_Num + 6)) // 채널수 x 2(2바이트 점유) x 샘플링 수량 + 6(파형데이터 구간 앞부분까지의 인덱스값) 
                        {
                            Sync_After = false; // 싱크지점 다시 검색되도록 false로 해둔다.
                            retv = 1; // 1패킷 단위의 파싱이 완료되면 리턴한다.
                        }
                    }
                } //if (Packet_TX_Index > 1)
            }
            return retv; // 1패킷이 완료되면 1을 반환, 그외에는 0반환.
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //this.Invoke(new EventHandler(MySerialReceived));
            byte data = MySerialReceive();

            Parsing_LXSDFT2(data);

        }

        private int MySerialReceive()
        {
            int ReceiveData = serialPort1.BytesToRead;  //시리얼 버터에 수신된 데이타를 ReceiveData 읽어오기
                                                        //Console.WriteLine("Data:" + string.Format("{0:X2}", ReceiveData));  //int 형식을 string형식으로 변환하여 출력
            Console.WriteLine("Data:" + string.Format("{0:X2}", ReceiveData));
            return ReceiveData;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)  //시리얼포트가 열려 있지 않으면
            {
                serialPort1.PortName = "COM3";  //콤보박스의 선택된 COM포트명을 시리얼포트명으로 지정
                serialPort1.BaudRate = 9600;  //보레이트 변경이 필요하면 숫자 변경하기
                serialPort1.DataBits = 8;
                serialPort1.StopBits = StopBits.One;
                serialPort1.Parity = Parity.None;
                serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived); //이것이 꼭 필요하다

                serialPort1.Open();  //시리얼포트 열기

            }
            else  //시리얼포트가 열려 있으면
            {
               
            }
        }
    }
}
