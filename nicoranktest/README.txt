■ 必要なライブラリ

・NUnit 2.4.8
http://www.nunit.org/
nunit.framework.dll を lib ディレクトリに配置。

・ShapZipLib 0.85.5
http://www.icsharpcode.net/OpenSource/SharpZipLib/
ICSharpCode.SharpZipLib.dll を lib ディレクトリに配置。

・WatiN 2.0.10.928
http://watin.sourceforge.net/
WatiN.Core.dll、Interop.SHDocVw.dll を lib ディレクトリに配置。

・JSSh 1.0
http://sourceforge.net/projects/ulti-swat/
上記 URL から Swat(Simple Web Automation Toolkit) をダウンロードして、
その中の jssh-Firefox35-WINNT.xpi を Firefox3.5 で開き、プラグインをイ
ンストールする。


■ テストデータ準備

testdata.txt を以下のように書き換える。

1) ok_mail と ok_pass にログイン可能なメールアドレスとパスワードを入力
   する

2) video_id に ok_mail に入力したアカウントで投稿した動画の動画IDを入
   力する。
   動画は、ダウンロード機能のテストでダウンロードするので、サイズは小
   さい方がいい。
   同様に、video_title、video_description、video_submit_date、
   video_length に投稿した動画の情報を入力する。

   テスト内で、video_id に設定した動画には以下のことが行われる
   ・タグの追加、削除
   ・コメントの投稿

3) video_id に入力した動画をあらかじめ何らかの方法でダウンロードしてお
   き、そのファイルパスを video_local に入力する。
   ファイルパスは絶対パスまたはテストを実行するプログラムのカレンドデ
   ィレクトリ(bin/Release、bin/Debug)からの相対パスを設定。

4) video_id に入力した動画のサムネイルをあらかじめダウンロードしておき、
   そのファイルパスを thumbnail_local に入力する。
   ファイルパスは絶対パスまたはテストを実行するプログラムのカレンドデ
   ィレクトリ(bin/Release、bin/Debug)からの相対パスを設定。

5) ok_mail に入力したアカウントであらかじめマイリストを作成し、そのマ
   イリストIDを mylist_id に入力する。
   作成したマイリストには video_id に入力した動画を登録しておく。

6) キーワード検索した時に video_id の動画が1ページ目にヒットするような
   検索ワードを search_keyword に入力する。
   検索結果が1ページになり、その中に video_id の動画が含まれるようにす
   ればよい。

7) タグ検索した時に video_id の動画が1ページ目にヒットするような検索ワ
   ードを search_tag に入力する。
   検索結果が1ページになり、その中に video_id の動画が含まれるようにす
   ればよい。
   video_id の動画にタグの追加/削除を行うテストもあるので、video_idに
   設定するタグは3つぐらいがよい。

   例) video_id が sm6852473 (http://www.nicovideo.jp/watch/sm6852473) 
       の場合
       search_keyword=A9FA877CDD 3840647C87 FAA938DD64
       search_tag=185687B140 714193B187 5618714093

   だたし、キーワード検索とタグ検索は、動画に設定してから検索結果に反
   映されるまでにある程度時間がかかるので、その間はテストが失敗してし
   まうので注意。
   (NicoNetworkTest の GetSearchKeywordTest と GetSearchTagTest)

8) nm6876660、nm6877305、so5558738 の動画をダウンロードし、そのファイ
   ルパスをそれぞれ video_local_nm、video_local_swf、video_local_so に
   入力する。
   ファイルパスは絶対パスまたはテストを実行するプログラムのカレンドデ
   ィレクトリ(bin/Release、bin/Debug)からの相対パスを設定。


■ テスト実行

・NUnit から実行
testdata.txt を bin/Release にコピーする。
NUnit をインストールし、nunit.exe を実行して、nicoranktest.nunit プロ
ジェクトを開く。
Run をクリックして実行する。

・Visual C# から実行
testdata.txt を bin/Debug にコピーする。
nicoranktest プロジェクトはコンソールアプリケーションになっているので、
Visual C# 内で実行することができる。
ただし、デフォルトではメソッドに TestAttribute が付いていても、自動で
は呼び出されない。
テストメソッドが呼び出されるようにするには、実行したいメソッドに
RunUnitTestAttribute を追加する。

例) [Test]
    public void MylistTest()

    ↓

    [Test]
    [RunUnitTest]
    public void MylistTest()


