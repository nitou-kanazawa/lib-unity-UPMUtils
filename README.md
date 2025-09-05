# UPM Utils

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

## 概要
Unity Package Manager には，エディタ用の[スクリプトAPI](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-api.html)が用意されている．
これらを利用して，使用頻度の高いパッケージの追加コマンドなどをエディタに追加する．

## 特徴



## セットアップ
#### 要件 / 開発環境
- Unity 6000.0

#### インストール

1. Window > Package ManagerからPackage Managerを開く
2. 「+」ボタン > Add package from git URL
3. 以下のURLを入力する
```
https://github.com/nitou-kanazawa/lib-unity-UPMUtils.git
```

あるいはPackages/manifest.jsonを開き、dependenciesブロックに以下を追記
```
{
    "dependencies": {
        "jp.nitou.upm-utils": "https://github.com/nitou-kanazawa/lib-unity-UPMUtils.git"
    }
}
```


## ドキュメント




## 導入方法


