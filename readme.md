#Plugin_VJoyInputer
##なにこれ？
指定した文字列の読み上げ時に仮想ゲームパッドへ入力を送信する、[棒読みちゃん](http://chi.usamimi.info/Program/Application/BouyomiChan/)（β版）用プラグインです。

##どうつかうの？

* [vJoy 公式サイト](http://vjoystick.sourceforge.net/site/)から最新の vJoy インストーラーをダウンロードしインストール（要再起動）。インストール後、vJoy の設定を行う。また、最新の FeederSDK もダウンロードしておく
* 棒読みちゃんのフォルダに本プラグイン(Plugin_VJoyInputer.dll)を配置
* 先ほどダウンロードした FeederSDK を展開し、SDK/C#/x86/ から vJoyInterface.dll および vJoyInterfaceWrap.dll をコピーしてきて本プラグインと同じ位置に配置
* 棒読みちゃんを起動し本プラグインを有効にして設定を行う（正規表現については[正規表現言語要素](http://msdn.microsoft.com/ja-jp/library/az24scfc(v=vs.90).aspx)を参照）


##動作環境

* Microsoft Windows 7 SP1(64bit)
* Microsoft .NET Framework 3.5 以上
* 棒読みちゃんβ版(Ver0.1.11.0 Beta15)
* vJoy(vJoy_x86x64_I030114 Version 2.0.2)
* FeederSDK(vJoy202SDK-011112 Version 2.0.1)

にて動作を確認しています。


##vJoyの設定について補足

* ボタン数は1から32個に対応
* POV Hat Switch 4 Directions 1つが十字キーに対応
* Basic Axis X と Basic Axis Y が左スティックのX軸とY軸に対応
* Basic Axis Z と Y軸 = Basic Axis Rz が右スティックのX軸とY軸に対応


##ライセンス
[License.txt](https://github.com/102782/Plugin_VJoyInputer/blob/master/License.txt) をご参照ください。


##連絡先
[https://twitter.com/102782](https://twitter.com/102782)