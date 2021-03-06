﻿#Plugin_VJoyInputer
##なにこれ？
指定した文字列の読み上げ時に仮想ゲームパッドへ入力を送信する、[棒読みちゃん](http://chi.usamimi.info/Program/Application/BouyomiChan/)（β版）用プラグインです。

##どうつかうの？

* [vJoy 公式サイト](http://vjoystick.sourceforge.net/site/)から最新の vJoy インストーラーをダウンロードしインストール（要再起動）。インストール後、vJoy の設定を行う。また、最新の FeederSDK もダウンロードしておく
* 棒読みちゃんのフォルダに本プラグイン(Plugin_VJoyInputer.dll)を配置
* 先ほどダウンロードした FeederSDK を展開し、SDK/C#/x86/ から vJoyInterface.dll および vJoyInterfaceWrap.dll をコピーしてきて本プラグインと同じ位置に配置
* 棒読みちゃんを起動し本プラグインを有効にして設定を行う（正規表現については[正規表現言語 - クイック リファレンス](https://docs.microsoft.com/ja-jp/dotnet/standard/base-types/regular-expression-language-quick-reference) を参照）


##動作環境

* Microsoft Windows 10 Pro(64bit)
* Microsoft .NET Framework 3.5 以上
* 棒読みちゃんβ版(Ver0.1.11.0 Beta16)
* vJoy(v2.1.8 Build 39)
* FeederSDK(2.1.8.39-270518/SDK)

にて動作を確認しています。


##vJoyの設定についての補足

* ボタン数は1から32個に対応
* POV Hat Switch 4 Directions 1つが十字キーに対応
* Basic Axis X と Basic Axis Y が左スティックのX軸とY軸に対応
* Basic Axis Z と Basic Axis Rz が右スティックのX軸とY軸に対応


##ライセンス
[License.txt](https://github.com/102782/Plugin_VJoyInputer/blob/master/License.txt) をご参照ください。


##連絡先
[https://twitter.com/102782](https://twitter.com/102782)