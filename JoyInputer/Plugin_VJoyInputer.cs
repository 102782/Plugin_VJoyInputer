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

namespace JoyInputer
{
    public class Plugin_VJoyInputer : IPlugin 
    {
        public string Name { get { return ""; } }

        public string Version { get { return ""; } }

        public string Caption { get { return ""; } }

        public ISettingFormData SettingFormData { get { return null; } } //プラグインの設定画面情報（設定画面が必要なければnullを返してください）

        //プラグイン開始時処理
        public void Begin()
        {

        }

        //プラグイン終了時処理
        public void End()
        {
            
        }
    }
}
