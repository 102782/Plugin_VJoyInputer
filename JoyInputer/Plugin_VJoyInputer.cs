using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;
using FNF.Utility;
using FNF.Controls;
using FNF.XmlSerializerSetting;
using FNF.BouyomiChanApp;
using System.Reflection;

namespace JoyInputer
{
    public class Plugin_VJoyInputer : IPlugin 
    {
        private string _InstalledDir = Base.CallAsmPath;
        private Settings_VJoyInputer _Settings; // 設定
        private SettingFormData_VJoyInputer _SettingFormData;
        private string _SettingFile = Base.CallAsmPath + Base.CallAsmName + ".setting"; // 設定ファイルの保存場所
        private SimpleLogger logger;
        private string _LogFile = Base.CallAsmPath + Base.CallAsmName + "_log.txt"; // logファイルの保存場所
        private VJoyController vjoy;
        private bool isInitializedVJoy { get; set; }
        
        public string Name { get { return "仮想ゲームパッド入力送信プラグイン"; } }
        public string Version { get { return "2014/02/20版"; } }
        public string Caption { get { return "ボタンごとに設定した正規表現にマッチするワードが読み上げられたとき、仮想ゲームパッドにボタン入力を送信します"; } }
        public ISettingFormData SettingFormData { get { return this._SettingFormData; } } // プラグインの設定画面情報（設定画面が必要なければnullを返してください）

        // プラグイン開始時処理
        public void Begin()
        {
            this.logger = new SimpleLogger();
            this.logger.Add(SimpleLogger.TAG.INFO, "Begin Plugin");

            // 設定ファイル読み込み
            this._Settings = new Settings_VJoyInputer(this);
            this._Settings.Load(this._SettingFile);
            this._SettingFormData = new SettingFormData_VJoyInputer(this._Settings);

            // vJoy使用の準備
            InitializeVJoy();

            // 読み上げ開始時のイベントを登録する
            ((BouyomiChan)((FormMain)Pub.FormMain).BC).TalkTaskStarted += (new EventHandler<BouyomiChan.TalkTaskStartedEventArgs>(this.TalkTaskStarted));
        }

        // プラグイン終了時処理
        public void End()
        {
            // 読み上げ開始時のイベントを取り除く
            ((BouyomiChan)((FormMain)Pub.FormMain).BC).TalkTaskStarted -= (new EventHandler<BouyomiChan.TalkTaskStartedEventArgs>(this.TalkTaskStarted));

            // vJoyの解放
            this.vjoy.Relinquish();
            this.isInitializedVJoy = false;
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Relinquished: vJoy device number {0}.", this.vjoy.id));

            // 設定ファイル保存
            this._Settings.Save(this._SettingFile);

            this.logger.Add(SimpleLogger.TAG.INFO, "End Plugin");
            // ログファイル保存
            this.logger.Save(this._LogFile);
        }

        // 設定クラス（設定画面表示・ファイル保存を簡略化。publicなメンバだけ保存される。XmlSerializerで処理できるクラスのみ使用可。）
        public class Settings_VJoyInputer : SettingsBase
        {
            // 保存される情報（設定画面からも参照される）
            public int span = 500;
            public string[] buttons = new string[32] 
            { 
                "^B$", "^A$", "^Y$", "^X$", "^L1$", "^L2$", "^R1$", "^R2$", 
                "^SELECT$", "^START$", "", "", "", "", "", "",
                "", "", "", "", "", "", "", "",
                "", "", "", "", "", "", "", ""
            };
            

            // 作成元プラグイン
            internal Plugin_VJoyInputer Plugin;

            // コンストラクタ
            public Settings_VJoyInputer()
            {
            }

            // コンストラクタ
            public Settings_VJoyInputer(Plugin_VJoyInputer inputer)
            {
                this.Plugin = inputer;
            }

            // GUIなどから当オブジェクトの読み込み(設定セーブ時・設定画面表示時に呼ばれる)
            public override void ReadSettings()
            {
                //this.Plugin.logger.Add(SimpleLogger.TAG.INFO, "Read Settings");
            }

            // 当オブジェクトからGUIなどへの反映(設定ロード時・設定更新時に呼ばれる)
            public override void WriteSettings()
            {
                //this.Plugin.logger.Add(SimpleLogger.TAG.INFO, "Write Settings");
            }
        }

        // 設定画面表示用クラス（設定画面表示・ファイル保存を簡略化。publicなメンバだけ保存される。XmlSerializerで処理できるクラスのみ使用可。）
        public class SettingFormData_VJoyInputer : ISettingFormData
        {
            Settings_VJoyInputer _Setting;

            public string Title { get { return _Setting.Plugin.Name; } }
            public bool ExpandAll { get { return false; } }
            public SettingsBase Setting { get { return _Setting; } }

            public SettingFormData_VJoyInputer(Settings_VJoyInputer setting)
            {
                this._Setting = setting;
                this.PBase = new SBase(_Setting);
            }

            // 設定画面で表示されるクラス(ISettingPropertyGrid)
            public SBase PBase;
            public class SBase : ISettingPropertyGrid
            {
                Settings_VJoyInputer _Setting;

                public SBase(Settings_VJoyInputer setting) 
                { 
                    this._Setting = setting; 
                }

                public string GetName()
                { 
                    return "設定"; 
                }

                [Category("基本設定")]
                [DisplayName("01)ボタンの押し込み時間(ミリ秒)")]
                [Description("設定した時間分だけ、一回の入力でボタンを押し続けます")]
                public int Span { get { return this._Setting.span; } set { this._Setting.span = value; } }

                #region ボタン設定
                [Category("ボタン設定")]
                [DisplayName("01)ボタン1")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン1を入力します")]
                public string button1 { get { return this._Setting.buttons[0]; } set { this._Setting.buttons[0] = value; } }

                [Category("ボタン設定")]
                [DisplayName("02)ボタン2")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン2を入力します")]
                public string button2 { get { return this._Setting.buttons[1]; } set { this._Setting.buttons[1] = value; } }

                [Category("ボタン設定")]
                [DisplayName("03)ボタン3")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン3を入力します")]
                public string button3 { get { return this._Setting.buttons[2]; } set { this._Setting.buttons[2] = value; } }

                [Category("ボタン設定")]
                [DisplayName("04)ボタン4")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン4を入力します")]
                public string button4 { get { return this._Setting.buttons[3]; } set { this._Setting.buttons[3] = value; } }

                [Category("ボタン設定")]
                [DisplayName("05)ボタン5")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン5を入力します")]
                public string button5 { get { return this._Setting.buttons[4]; } set { this._Setting.buttons[4] = value; } }

                [Category("ボタン設定")]
                [DisplayName("06)ボタン6")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン6を入力します")]
                public string button6 { get { return this._Setting.buttons[5]; } set { this._Setting.buttons[5] = value; } }

                [Category("ボタン設定")]
                [DisplayName("07)ボタン7")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン7を入力します")]
                public string button7 { get { return this._Setting.buttons[6]; } set { this._Setting.buttons[6] = value; } }

                [Category("ボタン設定")]
                [DisplayName("08)ボタン8")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン8を入力します")]
                public string button8 { get { return this._Setting.buttons[7]; } set { this._Setting.buttons[7] = value; } }

                [Category("ボタン設定")]
                [DisplayName("09)ボタン9")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン9を入力します")]
                public string button9 { get { return this._Setting.buttons[8]; } set { this._Setting.buttons[8] = value; } }

                [Category("ボタン設定")]
                [DisplayName("10)ボタン10")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン10を入力します")]
                public string button10 { get { return this._Setting.buttons[9]; } set { this._Setting.buttons[9] = value; } }

                [Category("ボタン設定")]
                [DisplayName("11)ボタン11")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン11を入力します")]
                public string button11 { get { return this._Setting.buttons[10]; } set { this._Setting.buttons[10] = value; } }

                [Category("ボタン設定")]
                [DisplayName("12)ボタン12")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン12を入力します")]
                public string button12 { get { return this._Setting.buttons[11]; } set { this._Setting.buttons[11] = value; } }

                [Category("ボタン設定")]
                [DisplayName("13)ボタン13")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン13を入力します")]
                public string button13 { get { return this._Setting.buttons[12]; } set { this._Setting.buttons[12] = value; } }

                [Category("ボタン設定")]
                [DisplayName("14)ボタン14")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン14を入力します")]
                public string button14 { get { return this._Setting.buttons[13]; } set { this._Setting.buttons[13] = value; } }

                [Category("ボタン設定")]
                [DisplayName("15)ボタン15")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン15を入力します")]
                public string button15 { get { return this._Setting.buttons[14]; } set { this._Setting.buttons[14] = value; } }

                [Category("ボタン設定")]
                [DisplayName("16)ボタン16")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン16を入力します")]
                public string button16 { get { return this._Setting.buttons[15]; } set { this._Setting.buttons[15] = value; } }

                [Category("ボタン設定")]
                [DisplayName("17)ボタン17")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン17を入力します")]
                public string button17 { get { return this._Setting.buttons[16]; } set { this._Setting.buttons[16] = value; } }

                [Category("ボタン設定")]
                [DisplayName("18)ボタン18")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン18を入力します")]
                public string button18 { get { return this._Setting.buttons[17]; } set { this._Setting.buttons[17] = value; } }

                [Category("ボタン設定")]
                [DisplayName("19)ボタン19")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン19を入力します")]
                public string button19 { get { return this._Setting.buttons[18]; } set { this._Setting.buttons[18] = value; } }

                [Category("ボタン設定")]
                [DisplayName("20)ボタン20")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン20を入力します")]
                public string button20 { get { return this._Setting.buttons[19]; } set { this._Setting.buttons[19] = value; } }

                [Category("ボタン設定")]
                [DisplayName("21)ボタン21")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン21を入力します")]
                public string button21 { get { return this._Setting.buttons[20]; } set { this._Setting.buttons[20] = value; } }

                [Category("ボタン設定")]
                [DisplayName("22)ボタン22")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン22を入力します")]
                public string button22 { get { return this._Setting.buttons[21]; } set { this._Setting.buttons[21] = value; } }

                [Category("ボタン設定")]
                [DisplayName("23)ボタン23")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン23を入力します")]
                public string button23 { get { return this._Setting.buttons[22]; } set { this._Setting.buttons[22] = value; } }

                [Category("ボタン設定")]
                [DisplayName("24)ボタン24")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン24を入力します")]
                public string button24 { get { return this._Setting.buttons[23]; } set { this._Setting.buttons[23] = value; } }

                [Category("ボタン設定")]
                [DisplayName("25)ボタン25")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン25を入力します")]
                public string button25 { get { return this._Setting.buttons[24]; } set { this._Setting.buttons[24] = value; } }

                [Category("ボタン設定")]
                [DisplayName("26)ボタン26")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン26を入力します")]
                public string button26 { get { return this._Setting.buttons[25]; } set { this._Setting.buttons[25] = value; } }

                [Category("ボタン設定")]
                [DisplayName("27)ボタン27")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン27を入力します")]
                public string button27 { get { return this._Setting.buttons[26]; } set { this._Setting.buttons[26] = value; } }

                [Category("ボタン設定")]
                [DisplayName("28)ボタン28")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン28を入力します")]
                public string button28 { get { return this._Setting.buttons[27]; } set { this._Setting.buttons[27] = value; } }

                [Category("ボタン設定")]
                [DisplayName("29)ボタン29")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン29を入力します")]
                public string button29 { get { return this._Setting.buttons[28]; } set { this._Setting.buttons[28] = value; } }

                [Category("ボタン設定")]
                [DisplayName("30)ボタン30")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン30を入力します")]
                public string button30 { get { return this._Setting.buttons[29]; } set { this._Setting.buttons[29] = value; } }

                [Category("ボタン設定")]
                [DisplayName("31)ボタン31")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン31を入力します")]
                public string button31 { get { return this._Setting.buttons[30]; } set { this._Setting.buttons[30] = value; } }

                [Category("ボタン設定")]
                [DisplayName("32)ボタン32")]
                [Description("読み上げられた文字列が指定した正規表現にマッチした時、ボタン32を入力します")]
                public string button32 { get { return this._Setting.buttons[31]; } set { this._Setting.buttons[31] = value; } }
                #endregion
            }
        }


        private void InitializeVJoy()
        {
            this.isInitializedVJoy = false;
            this.logger.Add(SimpleLogger.TAG.INFO, "Begin Initialize VJoyController");
            if (!File.Exists(this._InstalledDir + "/vJoyInterface.dll"))
            {
                this.logger.Add(SimpleLogger.TAG.ERROR, this._InstalledDir + "vJoyInterface.dll not found.Exit!");
                return;
            }

            if (!File.Exists(this._InstalledDir + "/vJoyInterfaceWrap.dll"))
            {
                this.logger.Add(SimpleLogger.TAG.ERROR, this._InstalledDir + "vJoyInterfaceWrap.dll not found.Exit!");
                return;
            }

            this.vjoy = new VJoyController(1);
            if (!this.vjoy.CheckDeviceID())
            {
                this.logger.Add(SimpleLogger.TAG.ERROR, string.Format("Illegal device ID {0}.Exit!", this.vjoy.id));
                return;
            }
            this.logger.Add(SimpleLogger.TAG.INFO, this.vjoy.GetDriverAttributes());
            this.logger.Add(SimpleLogger.TAG.INFO, this.vjoy.GetDeviceStatusMessage());
            if (this.vjoy.Acquire())
            {
                this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Acquired: vJoy device number {0}.", this.vjoy.id));
            }
            else
            {
                this.logger.Add(SimpleLogger.TAG.ERROR, string.Format("Failed to acquire vJoy device number {0}.Exit!", this.vjoy.id));
                return;
            }

            this.isInitializedVJoy = true;

            this.logger.Add(SimpleLogger.TAG.INFO, "End Initialize VJoyController");
        }

        private void TalkTaskStarted(object sender, BouyomiChan.TalkTaskStartedEventArgs e)
        {
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Event TalkTaskStarted {0}", (string)((TalkTaskEventArgs)e.TalkTask).SourceText));
        }
    }


}
