﻿using System;
using System.Net;
using System.Text;
using CommandLine;
using CommandLine.Text;
using HtmlAgilityPack;

namespace WebNovelTranslate
{
    class Program
    {
        public class Options
        {
            [Value(0, MetaName = "URL", Required = true, HelpText = "Base URL to download web novel")]
            public string BaseUrl { get; set; } = "";

            [Option('o', "out", Required = true, HelpText = "Output directory")]
            public string OutDir { get; set; } = "";

            [Option('v', "verbose", Default = false, Required = false, HelpText = "Set output to verbose messages")]
            public bool Verbose { get; set; } = false;
        }

        public class DownloadResults
        {
            public HttpStatusCode StatusCode { get; set; }
            public string ChapterTitle { get; set; } = "";
            public string Content { get; set; } = "";
        }

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            var results= Parser.Default.ParseArguments<Options>(args);
            if (results.Tag == ParserResultType.Parsed)
            {
                await results.WithParsedAsync(async opts => await RunOptionsAndReturnExitCode(opts));
            }
            else
            {
                results.WithNotParsed(errs => HandleParseError(results, errs));
            }
        }

        static async Task RunOptionsAndReturnExitCode(Options opts)
        {
            // ensure base url ends with '/'
            if (!opts.BaseUrl.EndsWith('/'))
            {
                opts.BaseUrl += '/';
            }

            // ensure output directory exists
            if (!System.IO.Directory.Exists(opts.OutDir))
            {
                System.IO.Directory.CreateDirectory(opts.OutDir);
            }

            System.IO.Directory.SetCurrentDirectory(opts.OutDir);

            // find the highest numbered file of the format '<OutDir>/chapter-<number>.txt'
            int currentChapter = 0;
            string prefix = Path.Combine(".", "chapter-");
            foreach (var file in System.IO.Directory.GetFiles(".", "*.txt"))
            {
                if (file.StartsWith(prefix))
                {
                    if (int.TryParse(file.AsSpan(prefix.Length, file.Length - prefix.Length - 4), out var chapter))
                    {
                        if (chapter > currentChapter)
                        {
                            currentChapter = chapter;
                        }
                    }
                }
            }

            var downloadResult = await GetChapterAsync(opts.BaseUrl, currentChapter + 1);
        }

        static async Task<DownloadResults> GetChapterAsync(string baseUrl, int chapter)
        {
            var pageContent =
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"ja\" lang=\"ja\" class=\"is-pc\">\n<head>\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />\n<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />\n<title>無気力ニートな元神童、冒険者になる\u3000\uff5e「学生時代の成績と実社会は別だろ？」と勘違いしたまま無自覚チートに無双する\uff5e - ２．元神童、帝都最大クラン『黒竜の牙』の求人に応募する</title>\n<meta http-equiv=\"Content-Script-Type\" content=\"text/javascript\" />\n<meta http-equiv=\"Content-Style-Type\" content=\"text/css\" />\n<meta name=\"format-detection\" content=\"telephone=no\" />\n\n<meta property=\"og:type\" content=\"website\" />\n<meta property=\"og:title\" content=\"無気力ニートな元神童、冒険者になる\u3000\uff5e「学生時代の成績と実社会は別だろ？」と勘違いしたまま無自覚チートに無双する\uff5e - ２．元神童、帝都最大クラン『黒竜の牙』の求人に応募する\" />\n<meta property=\"og:url\" content=\"https://ncode.syosetu.com/n3025gy/2/\" />\n<meta property=\"og:description\" content=\"R15 残酷な描写あり オリジナル戦記 ファンタジー 男主人公 チート 主人公最強 冒険者 ご都合主義 ざまぁ 成り上がり 無自覚 勘違い\" />\n<meta property=\"og:image\" content=\"https://sbo.syosetu.com/n3025gy/twitter.png\" />\n<meta property=\"og:site_name\" content=\"小説家になろう\" />\n<meta name=\"twitter:site\" content=\"@syosetu\">\n<meta name=\"twitter:card\" content=\"summary_large_image\">\n<meta name=\"twitter:creator\" content=\"ぺもぺもさん\">\n\n\n<link rel=\"shortcut icon\" href=\"https://static.syosetu.com/view/images/narou.ico?psawph\" />\n<link rel=\"icon\" href=\"https://static.syosetu.com/view/images/narou.ico?psawph\" />\n<link rel=\"apple-touch-icon-precomposed\" href=\"https://static.syosetu.com/view/images/apple-touch-icon-precomposed.png?ojjr8x\" />\n\n<link href=\"https://api.syosetu.com/writernovel/1527965.Atom\" rel=\"alternate\" type=\"application/atom+xml\" title=\"Atom\" />\n\n<link rel=\"stylesheet\" type=\"text/css\" href=\"https://static.syosetu.com/novelview/css/reset.css?piu6zr\" media=\"screen,print\" />\n<link rel=\"stylesheet\" type=\"text/css\" href=\"https://static.syosetu.com/view/css/lib/jquery.hina.css?oyb9lo\" media=\"screen,print\" />\n\n<link rel=\"stylesheet\" type=\"text/css\" media=\"all\" href=\"https://static.syosetu.com/view/css/lib/tippy.css?ov2lia\" />\n<link rel=\"stylesheet\" type=\"text/css\" media=\"all\" href=\"https://static.syosetu.com/novelview/css/siori_toast_pc.css?q6lspt\" />\n<link rel=\"stylesheet\" type=\"text/css\" media=\"all\" href=\"https://static.syosetu.com/view/css/lib/remodal.css?oqe20g\" />\n<link rel=\"stylesheet\" type=\"text/css\" media=\"all\" href=\"https://static.syosetu.com/view/css/lib/remodal-default_pc-theme.css?r9whxq\" />\n<link rel=\"stylesheet\" type=\"text/css\" media=\"all\" href=\"https://static.syosetu.com/novelview/css/remodal_pc.css?rfnxgm\" />\n<link rel=\"stylesheet\" type=\"text/css\" media=\"all\" href=\"https://static.syosetu.com/novelview/css/p_novelview-pc.css?sf9es3\" />\n<link rel=\"stylesheet\" type=\"text/css\" media=\"all\" href=\"https://static.syosetu.com/novelview/css/kotei.css?sd3mte\" />\n\n\n\n<script type=\"text/javascript\"><!--\nvar domain = 'syosetu.com';\n//--></script>\n\n<script type=\"text/javascript\" src=\"https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js\"></script>\n<script type=\"text/javascript\" src=\"https://static.syosetu.com/view/js/lib/jquery.hina.js?rq7apb\"></script>\n<script type=\"text/javascript\" src=\"https://static.syosetu.com/view/js/global.js?schjuj\"></script>\n<script type=\"text/javascript\" src=\"https://static.syosetu.com/view/js/char_count.js?scfmgz\"></script>\n<script type=\"text/javascript\" src=\"https://static.syosetu.com/novelview/js/novelview.js?sabqzs\"></script>\n\n<script type=\"text/javascript\" src=\"https://static.syosetu.com/view/js/lib/jquery.readmore.js?o7mki8\"></script>\n<script type=\"text/javascript\" src=\"https://static.syosetu.com/view/js/lib/tippy.min.js?oqe1mv\"></script>\n<script type=\"text/javascript\" src=\"https://static.syosetu.com/novelview/js/lib/jquery.raty.js?q6lspt\"></script>\n<script type=\"text/javascript\" src=\"https://static.syosetu.com/novelview/js/novel_bookmarkmenu.js?scucxy\"></script>\n<script type=\"text/javascript\" src=\"https://static.syosetu.com/novelview/js/novel_point.js?q6lspt\"></script>\n<script type=\"text/javascript\" src=\"https://static.syosetu.com/view/js/lib/remodal.min.js?oqe1mv\"></script>\n<script type=\"text/javascript\" src=\"https://static.syosetu.com/novelview/js/novel_good.js?r6m0tn\"></script>\n\n\n\n<script type=\"text/javascript\">\nvar microadCompass = microadCompass || {};\nmicroadCompass.queue = microadCompass.queue || [];\n</script>\n<script type=\"text/javascript\" charset=\"UTF-8\" src=\"//j.microad.net/js/compass.js\" onload=\"new microadCompass.AdInitializer().initialize();\" async></script>\n\n\n\n<script>\nwindow.gnshbrequest = window.gnshbrequest || {cmd:[]};\nwindow.gnshbrequest.cmd.push(function(){\nwindow.gnshbrequest.registerPassback(\"1563325\");\nwindow.gnshbrequest.registerPassback(\"1563326\");\nwindow.gnshbrequest.forceInternalRequest();\n});\n</script>\n<script async src=\"https://securepubads.g.doubleclick.net/tag/js/gpt.js\"></script>\n<script async src=\"https://cpt.geniee.jp/hb/v1/210776/476/wrapper.min.js\"></script>\n\n\n<!--\n<rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"\n xmlns:dc=\"http://purl.org/dc/elements/1.1/\">\n\n<rdf:Description\nrdf:about=\"https://ncode.syosetu.com/n3025gy/\ndc:identifier=\"https://ncode.syosetu.com/n3025gy/\"\n />\n</rdf:RDF>\n-->\n\n</head>\n<body onload=\"initRollovers();\">\n\n<a id=\"pageBottom\" href=\"#footer\">\u2193</a>\n\n<div id=\"novel_header\">\n<ul id=\"head_nav\">\n<li id=\"login\">\n<a href=\"https://syosetu.com/login/input/\"><span class=\"attention\">ログイン</span></a>\n</li>\n<li><a href=\"https://ncode.syosetu.com/novelview/infotop/ncode/n3025gy/\">作品情報</a></li>\n<li><a href=\"https://novelcom.syosetu.com/impression/list/ncode/1802845/no/2/\">感想</a></li>\n<li><a href=\"https://novelcom.syosetu.com/novelreview/list/ncode/1802845/\">レビュー</a></li>\n<li>\n<form action=\"https://ncode.syosetu.com/novelpdf/creatingpdf/ncode/n3025gy/\" class=\"js-pdf-form\" method=\"post\">\n<input type=\"submit\" value=\"縦書きPDF\" class=\"pdflilnk\" data-nolock=\"true\">\n<input type=\"hidden\" name=\"token\" value=\"d42486501c80105782c774161a4d2621\">\n</form>\n</li>\n<li class=\"booklist\">\n\n<span class=\"button_bookmark logout\">ブックマークに追加</span>\n\n<input type=\"hidden\" class=\"js-bookmark_url\" value=\"https://syosetu.com/favnovelmain/addajax/favncode/1802845/no/2/?token=d42486501c80105782c774161a4d2621\">\n</li>\n\n\n\n\n\n<li style=\"padding:5px 10px 5px;margin-top:11px;\">\n<a href=\"https://twitter.com/share?ref_src=twsrc%5Etfw\" class=\"twitter-share-button\" data-text=\"無気力ニートな元神童、冒険者になる\u3000\uff5e「学生時代の成績と実社会は別だろ？」と勘違いしたまま無自覚チートに無双する\uff5e / ２．元神童、帝都最大クラン『黒竜の牙』の求人に応募する\" data-url=\"https://ncode.syosetu.com/n3025gy/2/\" data-hashtags=\"narou,narouN3025GY\" data-lang=\"ja\" data-show-count=\"false\" style=\"border: none;\">Tweet</a><script async src=\"https://platform.twitter.com/widgets.js\" charset=\"utf-8\"></script>\n</li>\n\n</ul>\n<div id=\"novelnavi_right\">\n<div class=\"toggle\" id=\"menu_on\">表示調整</div>\n<div class=\"toggle_menuclose\" id=\"menu_off\">閉じる</div>\n\n<div class=\"novelview_navi\">\n<img src=\"https://static.syosetu.com/novelview/img/novelview_on.gif?n7nper\" width=\"100\" height=\"20\" style=\"cursor:pointer;\" onclick=\"sasieclick(true);\" id=\"sasieflag\" alt=\"挿絵表示切替ボタン\" />\n<script type=\"text/javascript\"><!--\nsasieinit();\n//--></script>\n\n<div class=\"color\">\n\u25bc配色<br />\n<label><input type=\"radio\" value=\"1\" name=\"colorset\" />標準設定</label><br />\n<label><input type=\"radio\" value=\"2\" name=\"colorset\" />ブラックモード</label><br />\n<label><input type=\"radio\" value=\"3\" name=\"colorset\" />ブラックモード2</label><br />\n<label><input type=\"radio\" value=\"4\" name=\"colorset\" />通常[1]</label><br />\n<label><input type=\"radio\" value=\"5\" name=\"colorset\" />通常[2]</label><br />\n<label><input type=\"radio\" value=\"6\" name=\"colorset\" />シンプル</label><br />\n<label><input type=\"radio\" value=\"7\" name=\"colorset\" />おすすめ設定</label>\n</div>\n\n\u25bc行間\n<ul class=\"novelview_menu\">\n<li><input type=\"text\" value=\"\" name=\"lineheight\" size=\"2\" style=\"text-align:right;\" readonly=\"readonly\" />％</li>\n<li><a href=\"javascript:void(0);\" name=\"lineheight_inc\" class=\"size\">+</a></li>\n<li><a href=\"javascript:void(0);\" name=\"lineheight_dec\" class=\"size\">-</a></li>\n<li><a href=\"javascript:void(0);\" name=\"lineheight_reset\">リセット</a></li>\n</ul>\n\n<div class=\"size\">\n\u25bc文字サイズ\n<ul class=\"novelview_menu\">\n<li><input type=\"text\" value=\"\" name=\"fontsize\" size=\"2\" style=\"text-align:right;\" readonly=\"readonly\" />％</li>\n<li><a href=\"javascript:void(0);\" name=\"fontsize_inc\" class=\"size\">+</a></li>\n<li><a href=\"javascript:void(0);\" name=\"fontsize_dec\" class=\"size\">-</a></li>\n<li><a href=\"javascript:void(0);\" name=\"fontsize_reset\">リセット</a></li>\n</ul>\n</div>\n\n<script type=\"text/javascript\"><!--\nchangeButtonView();\n//--></script>\n\n\u25bcメニューバー<br />\n<label><input type=\"checkbox\" name=\"fix_menu_bar\" />追従</label>\n<span id=\"menu_off_2\">\u00d7閉じる</span>\n\n</div><!--novelview_navi-->\n</div><!-- novelnavi_right -->\n</div><!--novel_header-->\n\n<div id=\"container\">\n\n\n\n<div class=\"box_announce_bookmark announce_bookmark\">\nブックマーク機能を使うには<a href=\"https://syosetu.com/login/input/\" target=\"_blank\">ログイン</a>してください。\n</div>\n\n<div class=\"contents1\">\n<a href=\"/n3025gy/\" class=\"margin_r20\">無気力ニートな元神童、冒険者になる\u3000\uff5e「学生時代の成績と実社会は別だろ？」と勘違いしたまま無自覚チートに無双する\uff5e</a>\n作者：<a href=\"https://mypage.syosetu.com/1527965/\">ぺもぺもさん</a>\n<p class=\"chapter_title\">第１章\u2212１</p>\n\n</div><!--contents1-->\n\n\n<div class=\"toaster bookmarker_addend\" id=\"toaster_success\">\n<strong>しおりの位置情報を変更しました</strong>\n</div>\n<div class=\"toaster bookmarker_error\" id=\"toaster_error\">\n<strong>エラーが発生しました</strong><br />\n<div class=\"text-right\">\n<a href=\"#\" class=\"toaster_close\">閉じる</a>\n</div>\n</div>\n\n<div class=\"narou_modal\" data-remodal-id=\"add_bookmark\">\n<div class=\"close js-add_bookmark_modal_close\"></div>\n\n<div class=\"scroll\">\n<p class=\"fav_noveltitle js-add_bookmark_title\"></p>\n<p class=\"mes\">ブックマークに追加しました</p>\n\n<div class=\"favnovelmain_update\">\n<h3>設定</h3>\n<p>\n<input type=\"checkbox\" name=\"isnotice\" value=\"1\" />更新通知\n<span class=\"left10 js-isnoticecnt\">0/400</span>\n</p>\n\n<p>\n<label><input type=\"radio\" name=\"jyokyo\" class=\"bookmark_jyokyo\" value=\"2\" checked=\"checked\" />公開</label>\n<label><input type=\"radio\" name=\"jyokyo\" class=\"bookmark_jyokyo left10\" value=\"1\" />非公開</label>\n</p>\n\n<div class=\"toaster bookmarker_addend js-bookmark_save_toaster\" id=\"bookmark_save_toaster\">\n<strong>設定を保存しました</strong>\n</div>\n\n<div class=\"toaster bookmarker_error js-bookmark_save_err_toaster\">\n<strong>エラーが発生しました</strong>\n<div id=\"bookmark_save_errmsg\" class=\"js-bookmark_save_errmsg\"></div>\n<div class=\"text-right\">\n<a href=\"#\" class=\"toaster_close\">閉じる</a>\n</div>\n</div>\n\n<p>\nカテゴリ\n<select name=\"categoryid\" class=\"js-category_select\"></select>\n</p>\n<div class=\"favnovelmain_bkm_memo\">\n<label>メモ</label>\n<div class=\"favnovelmain_bkm_memo_content\">\n<input type=\"text\" maxlength=\"\" class=\"js-bookmark_memo\">\n<span class=\"favnovelmain_bkm_memo_content_help-text\">※文字以内</span>\n</div>\n</div>\n\n<input type=\"button\" class=\"button js-bookmark_setting_submit\" value=\"設定を保存\" />\n<input type=\"hidden\" class=\"js-bookmark_setting_url\" value=\"https://syosetu.com/favnovelmain/updateajax/\" />\n<input type=\"hidden\" class=\"js-bookmark_setting_useridfavncode\" value=\"\" />\n<input type=\"hidden\" class=\"js-bookmark_setting_xidfavncode\" value=\"\" />\n<input type=\"hidden\" class=\"js-bookmark_setting_token\" value=\"\" />\n</div>\n<!-- favnovelmain_update -->\n\n<a href=\"https://syosetu.com/favnovelmain/list/\" class=\"button js-modal_bookmark_btn\">ブックマークへ移動</a>\n<input type=\"hidden\" value=\"//syosetu.com/favnovelmain/list/\" class=\"js-base_bookmark_url\">\n</div><!-- scroll -->\n</div>\n<!-- narou_modal -->\n\n<div class=\"narou_modal\" data-remodal-id=\"delend_siori\">\n<div class=\"close js-delend_siori_modal_close\"></div>\n<p class=\"mes\">しおりを解除しました。</p>\n</div>\n\n<div class=\"narou_modal\" data-remodal-id=\"delend_bookmark\">\n<div class=\"close js-delend_bookmark_modal_close\"></div>\n<p class=\"mes\">ブックマークを解除しました。</p>\n</div>\n\n<div class=\"narou_modal modal_error\" data-remodal-id=\"error_modal\">\n<div class=\"close js-error_modal_close\"></div>\n\n<div class=\"scroll\">\n<p class=\"mes err\">エラーが発生しました。</p>\n<div class=\"err js-modal_err_mes\"></div>\n<p class=\"guide\">エラーの原因がわからない場合は<a href=\"https://syosetu.com/helpcenter/top/\" target=\"_blank\">ヘルプセンター</a>をご確認ください。</p>\n</div><!-- scroll -->\n</div>\n<div id=\"novel_contents\">\n<div id=\"novel_color\">\n\n\n\n<div class=\"novel_bn\">\n<a href=\"/n3025gy/1/\" class=\"novelview_pager-before\">&nbsp;前へ</a><a href=\"/n3025gy/3/\" class=\"novelview_pager-next\">次へ&nbsp;</a></div><!--novel_bn-->\n\n<div class=\"koukoku_728\">\n\n<div id=\"67f6d369978ef601fa2de6b3fb770466\" >\n<script type=\"text/javascript\">\nmicroadCompass.queue.push({\n\"spot\": \"67f6d369978ef601fa2de6b3fb770466\"\n});\n</script>\n</div>\n\n\n</div><!--koukoku_728-->\n\n<div id=\"novel_no\">2/55</div>\n\n\n<p class=\"novel_subtitle\">２．元神童、帝都最大クラン『黒竜の牙』の求人に応募する</p>\n\n\n\n<div id=\"novel_honbun\" class=\"novel_view\">\n<p id=\"L1\">\u3000そんなわけで翌日、俺は帝都最大クラン『黒竜の牙』の試験会場を訪れた。</p>\n<p id=\"L2\"><br /></p>\n<p id=\"L3\">\u3000さすがは帝都最大クランの募集だけある。剣を腰に差した戦士やローブをまとった魔術師、はたまた革の鎧に身を包んだ斥候など多くの冒険者たちが集まっていた。</p>\n<p id=\"L4\">\u3000目つきからしてぎらぎらしている。このクランに入って名を上げるのは俺たちだ！\u3000みたいな空気が立ち上っている。</p>\n<p id=\"L5\"><br /></p>\n<p id=\"L6\">\u3000俺の場違い感がすごい。</p>\n<p id=\"L7\">\u3000俺の装備は『布の服』のみ。もちろん、武器なんて持ってきていない。</p>\n<p id=\"L8\"><br /></p>\n<p id=\"L9\">\u3000受験するには申し込み用紙の提出が必要らしい。俺はそれを書くためのコーナーへと足を向けた。</p>\n<p id=\"L10\">\u3000用紙を順に埋めていく。</p>\n<p id=\"L11\">\u3000ふむふむ……。</p>\n<p id=\"L12\">\u3000名前はイルヴィス、性別は男、年齢は２０歳、職業――</p>\n<p id=\"L13\">\u3000職業！？</p>\n<p id=\"L14\">\u3000困った。特に仕事はしていないのだが……家でごろごろしていたから……『ニート』だろうか？\u3000ニートだな……。</p>\n<p id=\"L15\">\u3000カッコよくデタラメを書こうかとも思ったが、やめた。</p>\n<p id=\"L16\">\u3000就職マニュアル『内定無双』にも書いてあったじゃないか。面接担当は鷹の耳目を持つと。嘘など書いても一瞬で見破られて評価が下がるだけと。</p>\n<p id=\"L17\">\u3000なので、ここは正直に書くのが正しいだろう。</p>\n<p id=\"L18\"><br /></p>\n<p id=\"L19\">\u3000俺は職業欄に『ニート』と書いた。</p>\n<p id=\"L20\"><br /></p>\n<p id=\"L21\">\u3000それを受付へと持っていく。</p>\n<p id=\"L22\">\u3000受付にいる女性は俺が提出した紙を見た瞬間、眉をひそめた。</p>\n<p id=\"L23\"><br /></p>\n<p id=\"L24\">「ニー……ト？」</p>\n<p id=\"L25\"><br /></p>\n<p id=\"L26\">「はい」</p>\n<p id=\"L27\"><br /></p>\n<p id=\"L28\">「ニートって、あの、働いたら負けだと思う感じの？」</p>\n<p id=\"L29\"><br /></p>\n<p id=\"L30\">「そうですね」</p>\n<p id=\"L31\"><br /></p>\n<p id=\"L32\">「……えーとですね、ここは冒険者としての職業を書いて欲しいんですよ。戦士とか盗賊とか」</p>\n<p id=\"L33\"><br /></p>\n<p id=\"L34\">「なるほど」</p>\n<p id=\"L35\"><br /></p>\n<p id=\"L36\">\u3000そっちだったか。だが、一緒だ。ただの学生でしかなかった俺にそんな職業はない。</p>\n<p id=\"L37\">\u3000なので、きっぱりと言い切った。</p>\n<p id=\"L38\"><br /></p>\n<p id=\"L39\">「だとしたら、やっぱりニートですね」</p>\n<p id=\"L40\"><br /></p>\n<p id=\"L41\">\u3000女性が混乱した表情を浮かべた。</p>\n<p id=\"L42\"><br /></p>\n<p id=\"L43\">「……過去に冒険者をされていて、何かしらの理由で今はニートをされている感じでしょうか？」</p>\n<p id=\"L44\"><br /></p>\n<p id=\"L45\">「いえ、冒険者はしたことないです。本当にただのニートです」</p>\n<p id=\"L46\"><br /></p>\n<p id=\"L47\">「え？」</p>\n<p id=\"L48\"><br /></p>\n<p id=\"L49\">「え？」</p>\n<p id=\"L50\"><br /></p>\n<p id=\"L51\">「冒険者をされていないんですか？\u3000であれば、申し訳ないのですが、当クランは未経験者を採用していないんですよ」</p>\n<p id=\"L52\"><br /></p>\n<p id=\"L53\">\u3000わかりました。じゃあ、帰ります。</p>\n<p id=\"L54\">\u3000やる気のない俺的には、ラッキー！\u3000と思ったが、いきなり脳内に怒ったアリサの顔が浮かんで寒気がした。</p>\n<p id=\"L55\">\u3000……もうちょっと頑張ろう……。</p>\n<p id=\"L56\"><br /></p>\n<p id=\"L57\">「そうなんですか？」</p>\n<p id=\"L58\"><br /></p>\n<p id=\"L59\">\u3000俺はポケットからチラシを取り出した。</p>\n<p id=\"L60\"><br /></p>\n<p id=\"L61\">「でも、そんなこと書いていませんよ？\u3000むしろ『懇切丁寧に指導します』って書いていますよね？」</p>\n<p id=\"L62\"><br /></p>\n<p id=\"L63\">\u3000俺のツッコミに受付女性の眉がゆがむ。</p>\n<p id=\"L64\"><br /></p>\n<p id=\"L65\">「確かにおっしゃるとおりですけど、『経験の浅い冒険者でも歓迎』とも書いていますよね？」</p>\n<p id=\"L66\"><br /></p>\n<p id=\"L67\">「経験の浅い冒険者を歓迎するのは、未経験者を拒否することを意味しませんよね？」</p>\n<p id=\"L68\"><br /></p>\n<p id=\"L69\">\u3000さらなる俺のツッコミに受付女性の眉が深刻にゆがむ。</p>\n<p id=\"L70\">\u3000なんだか面倒なやつだな、俺……でも、アリサのプレッシャーがあるからさ。ごめんなさい。</p>\n<p id=\"L71\">\u3000少し考えてから女性が口を開こうとすると――</p>\n<p id=\"L72\"><br /></p>\n<p id=\"L73\">「列の流れが止まっているよ。何かあったのかい？」</p>\n<p id=\"L74\"><br /></p>\n<p id=\"L75\">\u3000声とともに男が近づいてきた。</p>\n<p id=\"L76\"><br /></p>\n<p id=\"L77\">「あ、すみません、フォニックさま！」</p>\n<p id=\"L78\"><br /></p>\n<p id=\"L79\">\u3000女性がぱっと立ち上がり、男に頭を下げる。</p>\n<p id=\"L80\">\u3000背の高い、青色の髪をした優男だった。年は２０半ばくらいか。筋肉が過不足なくついた細身の身体に軽装の鎧をまとい、腰に剣を差している。</p>\n<p id=\"L81\">\u3000背後の受験生たちがどよめいた。</p>\n<p id=\"L82\"><br /></p>\n<p id=\"L83\">「『黒竜の牙』のフォニックだ！」</p>\n<p id=\"L84\"><br /></p>\n<p id=\"L85\">「流星の剣士フォニックか！」</p>\n<p id=\"L86\"><br /></p>\n<p id=\"L87\">\u3000……。</p>\n<p id=\"L88\">\u3000……誰……？</p>\n<p id=\"L89\"><br /></p>\n<p id=\"L90\">\u3000どうやら有名な人物らしいが、思いつきで冒険者になろうと思った俺の頭脳に著名な冒険者の情報などあるはずもない。</p>\n<p id=\"L91\">\u3000女性はフォニックに用紙を見せた後、早口で状況を説明する。</p>\n<p id=\"L92\"><br /></p>\n<p id=\"L93\">「あの、実はこの方が――」</p>\n<p id=\"L94\"><br /></p>\n<p id=\"L95\">\u3000それを聞き終えたフォニックが俺に視線を向けた。</p>\n<p id=\"L96\"><br /></p>\n<p id=\"L97\">「悪いが、参加は見合わせてもらえないかな？\u3000これは君のためでもある」</p>\n<p id=\"L98\"><br /></p>\n<p id=\"L99\">「俺のため？」</p>\n<p id=\"L100\"><br /></p>\n<p id=\"L101\">「ああ。冒険者の試験とは――」</p>\n<p id=\"L102\"><br /></p>\n<p id=\"L103\">\u3000きん、と音を立ててフォニックが剣を引き抜いた。</p>\n<p id=\"L104\">\u3000陽光の輝きを受けて、磨き抜かれた美しい刀身がきらりと光る。</p>\n<p id=\"L105\"><br /></p>\n<p id=\"L106\">「荒事だからね。準備ができていなければケガをする。鍛錬していない人間が立てば――死ぬよ」</p>\n<p id=\"L107\"><br /></p>\n<p id=\"L108\">「死ぬ覚悟ならできていますよ」</p>\n<p id=\"L109\"><br /></p>\n<p id=\"L110\">\u3000俺はあっさりと言い放った。</p>\n<p id=\"L111\"><br /></p>\n<p id=\"L112\">「俺にはこれしかありませんから」</p>\n<p id=\"L113\"><br /></p>\n<p id=\"L114\">\u3000働くことが嫌な俺には自由業しかない。最低限の労働で収入ゲット、あとは家でゴロゴロ。思いつくのは冒険者しかないのだから、必死にもなる。</p>\n<p id=\"L115\">\u3000フォニックが少し目を見開いた。</p>\n<p id=\"L116\"><br /></p>\n<p id=\"L117\">「ほぅ、この私がここまで言っても引かないか。君には何か……冒険者への熱い想い――気高い理想があるようだな。少し興味が出てきたよ」</p>\n<p id=\"L118\"><br /></p>\n<p id=\"L119\">\u3000気高い理想……？</p>\n<p id=\"L120\">\u3000家でゴロゴロすることが？</p>\n<p id=\"L121\">\u3000まあ、居心地のよいワークライフバランスを追求するという意味ではそうかもしれないが。なるほど、そこまで読んでの言葉か……。さすがは最大手クランのメンバー、頭の回転が速い。</p>\n<p id=\"L122\">\u3000ならば、俺も胸を張って答えよう。</p>\n<p id=\"L123\"><br /></p>\n<p id=\"L124\">「そうですね、俺にも譲れないものがあるんです」</p>\n<p id=\"L125\"><br /></p>\n<p id=\"L126\">「はははは！\u3000いいね！」</p>\n<p id=\"L127\"><br /></p>\n<p id=\"L128\">\u3000フォニックが俺に剣の切っ先を向けた。</p>\n<p id=\"L129\"><br /></p>\n<p id=\"L130\">「だけど、現実は甘くない。努力もせず理想を語る人間は見ていて気分のいいものではない。鍛え抜かれた私の一閃を見ても同じことが言えるかな？」</p>\n<p id=\"L131\"><br /></p>\n<p id=\"L132\">\u3000周りの冒険者たちが沸きたつ。</p>\n<p id=\"L133\"><br /></p>\n<p id=\"L134\">「フォニックの――流星の剣が見れるぞ！」</p>\n<p id=\"L135\"><br /></p>\n<p id=\"L136\">「斬られたことすら気づけないほどの高速剣！」</p>\n<p id=\"L137\"><br /></p>\n<p id=\"L138\">\u3000……え、そんなにすごいの？</p>\n<p id=\"L139\">\u3000怒らせちゃいけない人、怒らせちゃった？</p>\n<p id=\"L140\">\u3000フォニックが剣を構える。</p>\n<p id=\"L141\"><br /></p>\n<p id=\"L142\">「さて、ここまで来た駄賃だ。私の剣技を見せてあげよう……そして、腰を抜かして帰るがいい」</p>\n<p id=\"L143\"><br /></p>\n<p id=\"L144\">「腰を抜かしたら帰れないんじゃないですか？」</p>\n<p id=\"L145\"><br /></p>\n<p id=\"L146\">「ぬかせ！」</p>\n<p id=\"L147\"><br /></p>\n<p id=\"L148\">\u3000腰を抜かしたらに『ぬかせ』で返すなんて！</p>\n<p id=\"L149\">\u3000剣技以外も冴え渡ってるんじゃないですか！？</p>\n<p id=\"L150\"><br /></p>\n<p id=\"L151\">\u3000そんなことを俺が思うと同時――</p>\n<p id=\"L152\">\u3000銀色の閃光が走った。</p>\n<p id=\"L153\"><br /></p>\n<p id=\"L154\">\u3000周りの冒険者たちが言うところの、『斬られたことにすら気づけない』超高速の剣が俺に襲いかかる。</p>\n<p id=\"L155\">\u3000俺はフォニックの剣を見ながら思った。</p>\n<p id=\"L156\"><br /></p>\n<p id=\"L157\">\u3000……。</p>\n<p id=\"L158\">\u3000……。</p>\n<p id=\"L159\"><br /></p>\n<p id=\"L160\">\u3000……え、これが超高速なの？</p>\n<p id=\"L161\">\u3000むちゃくちゃ遅いんだが。</p>\n<p id=\"L162\"><br /></p>\n<p id=\"L163\">\u3000刃がゆっくりとゆっくりと、少しずつ俺に向かってくる。</p>\n<p id=\"L164\">\u3000懐かしいな――ああ、思い出した。学生時代の剣術の授業だ。俺はいろいろな学生たちと剣を交わしたわけだが、総じてみんなこんな感じだった。</p>\n<p id=\"L165\">\u3000みんな剣が遅くて遅くて――俺はあっさりと隙をついて勝ったものだ。</p>\n<p id=\"L166\">\u3000これも同じか？</p>\n<p id=\"L167\"><br /></p>\n<p id=\"L168\">\u3000いや……違う。</p>\n<p id=\"L169\"><br /></p>\n<p id=\"L170\">\u3000就職マニュアル『内定無双』に書いてあったではないか。</p>\n<p id=\"L171\">\u3000学生時代の栄光は捨て去れと。</p>\n<p id=\"L172\">\u3000学生と社会人は求められるレベルが違う――</p>\n<p id=\"L173\"><br /></p>\n<p id=\"L174\">\u3000相手は最大手クランに所属する有名な冒険者なのだ。学生と同じはずがない。危ない危ない……ついつい学生時代の栄光から相手の力量を推し量ってしまうところだった。</p>\n<p id=\"L175\"><br /></p>\n<p id=\"L176\">\u3000であれば可能性はひとつしかない。</p>\n<p id=\"L177\"><br /></p>\n<p id=\"L178\">\u3000これは手を抜いてくれているのだ。</p>\n<p id=\"L179\"><br /></p>\n<p id=\"L180\">\u3000あれだけプレッシャーをかけつつも、フォニックは俺に情をかけてくれている。誰もが一目置くフォニックの</p>\n<p id=\"L181\">剣をかわせば俺にだって受験のチャンスがやってくるだろう。</p>\n<p id=\"L182\">\u3000ゆっくりとした剣からフォニックの気持ちが伝わってくるようだ。</p>\n<p id=\"L183\">\u3000どうだい？\u3000これならかわせるだろう？\u3000このチャンスをいかしてくれよ？\u3000と。</p>\n<p id=\"L184\"><br /></p>\n<p id=\"L185\">\u3000なるほど――</p>\n<p id=\"L186\">\u3000実にありがたい。</p>\n<p id=\"L187\"><br /></p>\n<p id=\"L188\">\u3000さて、どうやってかわそうかな……と俺が考えていると、俺はあることに気がついた。</p>\n<p id=\"L189\">\u3000……あれ？\u3000この軌道、俺に当たらないんじゃない？</p>\n<p id=\"L190\">\u3000俺の想像どおり、流星の剣士が放った剣の軌道は俺の身体の少し手前をなぞっていった。</p>\n<p id=\"L191\">\u3000空振り。</p>\n<p id=\"L192\">\u3000しんと静まった空気に、フォニックの声が響く。</p>\n<p id=\"L193\"><br /></p>\n<p id=\"L194\">「……当てるわけにもいかないからね。わざと外させてもらったよ」</p>\n<p id=\"L195\"><br /></p>\n<p id=\"L196\">\u3000同時――</p>\n<p id=\"L197\">\u3000わあああああ！\u3000と周囲のギャラリーが沸き立った。</p>\n<p id=\"L198\"><br /></p>\n<p id=\"L199\">「すげえええ！\u3000さすがは流星の剣！」</p>\n<p id=\"L200\"><br /></p>\n<p id=\"L201\">「あいつ、ビビって動けなかったぞ！」</p>\n<p id=\"L202\"><br /></p>\n<p id=\"L203\">\u3000だが、フォニックの認識は違ったようだ。</p>\n<p id=\"L204\"><br /></p>\n<p id=\"L205\">「……いや、どうかな。君は動けなかったのではなく、動かなかった。違うか？」</p>\n<p id=\"L206\"><br /></p>\n<p id=\"L207\">\u3000フォニックがじっと俺を見る。</p>\n<p id=\"L208\"><br /></p>\n<p id=\"L209\">「君の目は確かに私の剣に反応していた。もしも、かわそうと思えばかわせた――違うな。当たらないことを確信していた。そうだろう？」</p>\n<p id=\"L210\"><br /></p>\n<p id=\"L211\">「はい、そうですね」</p>\n<p id=\"L212\"><br /></p>\n<p id=\"L213\">\u3000同時、周りのギャラリーたちに動揺が走る。</p>\n<p id=\"L214\"><br /></p>\n<p id=\"L215\">「そ、そんな！\u3000あいつが流星の剣士の剣を！？」</p>\n<p id=\"L216\"><br /></p>\n<p id=\"L217\">「俺たちには見えなかったのに！？」</p>\n<p id=\"L218\"><br /></p>\n<p id=\"L219\">\u3000……見えなかった？\u3000そんなわけないだろ？\u3000あそこまで手を抜いてくれているのに。</p>\n<p id=\"L220\">\u3000認知バイアスというやつか。</p>\n<p id=\"L221\">\u3000流星の剣士の剣は速い、その思い込みが彼らに超高速の剣を見せたのだろう。</p>\n<p id=\"L222\"><br /></p>\n<p id=\"L223\">「面白い男だな、君は。よし、受験を認めようではないか」</p>\n<p id=\"L224\"><br /></p>\n<p id=\"L225\">\u3000ふっとフォニックが笑う。剣を鞘に収めた。</p>\n<p id=\"L226\"><br /></p>\n<p id=\"L227\">「剣技の試験では私がじきじきに相手をしてあげよう。果たして、君のそれが実力なのかマグレなのか――真価を見定めさせてもらう」</p>\n<p id=\"L228\"><br /></p>\n<p id=\"L229\">\u3000そう言うとフォニックは建物の奥へと消えていった。</p>\n<p id=\"L230\"><br /></p>\n<p id=\"L231\">\u3000俺はその背中に心中で小さく礼を述べる。</p>\n<p id=\"L232\">\u3000……やれやれ……優しい人に恵まれた。</p>\n<p id=\"L233\">\u3000俺が腰を抜かさないようにゆっくりと斬りかかってきてくれて――おまけに、それはかわす必要がない斬撃で。さらには周りに俺が優秀だと吹聴するアピールまでしてくれる。\u3000</p>\n<p id=\"L234\">\u3000どれほど譲ってくれているのだ、あの御仁は。聖人か。</p>\n<p id=\"L235\">\u3000フォニックの許可が出たので、態度を軟化させた受付嬢から受験者番号をもらった。これで手続きは完了した。</p>\n<p id=\"L236\"><br /></p>\n<p id=\"L237\">\u3000よーし！</p>\n<p id=\"L238\">\u3000アリサ、俺、頑張ってくるからな！</p>\n<p id=\"L239\"><br /></p>\n<p id=\"L240\">\u3000俺は意気揚々と試験会場へと入っていった。</p>\n<p id=\"L241\"><br /></p>\n</div>\n\n\n\n<div class=\"novel_bn\">\n<a href=\"/n3025gy/1/\" class=\"novelview_pager-before\">&nbsp;前へ</a><a href=\"/n3025gy/3/\" class=\"novelview_pager-next\">次へ&nbsp;</a><a href=\"https://ncode.syosetu.com/n3025gy/\">目次</a></div>\n\n</div><!--novel_color-->\n\n\n<div class=\"koukoku_300x2\">\n<div class=\"koukoku_300x2__ad\">\n\n<div id=\"73f5392834d0169d96943c0a5a1ee084\" >\n<script type=\"text/javascript\">\nmicroadCompass.queue.push({\n\"spot\": \"73f5392834d0169d96943c0a5a1ee084\"\n});\n</script>\n</div>\n\n\n</div><!-- /.koukoku_300x2__ad -->\n<div class=\"koukoku_300x2__ad\">\n\n<div id=\"80d0b7e89fff615220d533a9b8e8a67b\" >\n<script type=\"text/javascript\">\nmicroadCompass.queue.push({\n\"spot\": \"80d0b7e89fff615220d533a9b8e8a67b\"\n});\n</script>\n</div>\n\n\n</div><!-- /.koukoku_300x2__ad -->\n</div><!-- /.koukoku_300x2 -->\n\n\n<div class=\"wrap_menu_novelview_after\">\n<div class=\"box_menu_novelview_after clearfix\">\n<ul class=\"menu_novelview_after\">\n\n<li class=\"list_menu_novelview_after\"><a href=\"https://syosetu.com/favnovelmain/list/\">ブックマーク</a></li>\n\n\n<li class=\"list_menu_novelview_after\"><a href=\"JavaScript:void(0);\" class=\"js-scroll\" data-scroll=\"#impression\">感想を書く</a></li>\n</ul>\n\n<ul class=\"footerbookmark\"><li class=\"booklist\">\n\n<span class=\"button_bookmark logout\">ブックマークに追加</span>\n\n<input type=\"hidden\" class=\"js-bookmark_url\" value=\"https://syosetu.com/favnovelmain/addajax/favncode/1802845/no/2/?token=d42486501c80105782c774161a4d2621\">\n</li>\n\n</ul>\n</div><!-- footer_bookmark -->\n</div><!-- wrap -->\n\n\n<div class=\"box_announce_bookmark announce_bookmark\">\nブックマーク機能を使うには<a href=\"https://syosetu.com/login/input/\" target=\"_blank\">ログイン</a>してください。\n</div>\n\n\n<div class=\"p-novelgood-form\">\n<div class=\"p-novelgood-form__wrap\">\n<div class=\"p-novelgood-form__discription customlayout-color\">\n<div class=\"p-novelgood-form__info\">\n<span>いいねで応援</span><a href=\"https://syosetu.com/helpcenter/helppage/helppageid/107\" target=\"_blank\"><span class=\"p-icon p-icon--question\" aria-label=\"ヘルプ\"></span></a>\n</div>\n<span class=\"p-novelgood-form__status\">受付停止中</span>\n</div>\n<div class=\"p-novelgood-form__icon p-novelgood-form__icon--stop\"><span class=\"p-icon p-icon--thumbs-up-line-off\"></span></div>\n</div><!-- /.p-novelgood-form__wrap -->\n</div><!-- /.p-novelgood-form -->\n\n<div id=\"novel_hyouka\" class=\"p-novelpoint-form\">\n<input type=\"hidden\" name=\"img_star_half\" value=\"is-half\" />\n<input type=\"hidden\" name=\"img_star_off\" value=\"is-empty\" />\n<input type=\"hidden\" name=\"img_star_on\" value=\"is-full\" />\n<div class=\"p-novelpoint-form__description\">\nポイントを入れて作者を応援しましょう！<a href=\"https://syosetu.com/helpcenter/helppage/helppageid/122\" target=\"_blank\"><span class=\"p-icon p-icon--question\" aria-label=\"ヘルプ\"></span></a>\n</div>\n<input type=\"hidden\" name=\"before_hyokapoint\" value=0 />\n<div id=\"novelpoint_notactive\" class=\"c-novelpoint-star c-novelpoint-star--lg c-novelpoint-star--notactive\"></div>\n<div class=\"p-novelpoint-form__note p-novelpoint-form__note--attention\">\n評価をするには<a href=\"https://syosetu.com/login/input/\">ログイン</a>してください。\n</div>\n</div><!-- novel_hyouka -->\n\n<div class=\"center novelrankingtag\">\n<p style=\"font-size:larger;\">\n</p><p style=\"font-size:larger;\">\nコミック版無気力ニート、発売中です(2023/01)！<br /></p>\n<img src=\"https://34462.mitemin.net/userpageimage/viewimage/icode/i710786/\" alt=\"shoei\" /><br /><br /><br /><p style=\"font-size:larger;\">文庫１巻、発売します（２０２２年５月２５日）！\u3000<br />第０章『神童、就活してニートになる』を加筆。</p>\n\n<img src=\"https://34462.mitemin.net/userpageimage/viewimage/icode/i647904/\" alt=\"shoei2\" /><br /><br />\n</div>\n\n\n\n\n<br />\n\n<div class=\"center\">\n<a href=\"https://twitter.com/intent/tweet?text=%E3%80%8C%E7%84%A1%E6%B0%97%E5%8A%9B%E3%83%8B%E3%83%BC%E3%83%88%E3%81%AA%E5%85%83%E7%A5%9E%E7%AB%A5%E3%80%81%E5%86%92%E9%99%BA%E8%80%85%E3%81%AB%E3%81%AA%E3%82%8B%E3%80%80%EF%BD%9E%E3%80%8C%E5%AD%A6%E7%94%9F%E6%99%82%E4%BB%A3%E3%81%AE%E6%88%90%E7%B8%BE%E3%81%A8%E5%AE%9F%E7%A4%BE%E4%BC%9A%E3%81%AF%E5%88%A5%E3%81%A0%E3%82%8D%EF%BC%9F%E3%80%8D%E3%81%A8%E5%8B%98%E9%81%95%E3%81%84%E3%81%97%E3%81%9F%E3%81%BE%E3%81%BE%E7%84%A1%E8%87%AA%E8%A6%9A%E3%83%81%E3%83%BC%E3%83%88%E3%81%AB%E7%84%A1%E5%8F%8C%E3%81%99%E3%82%8B%EF%BD%9E%E3%80%8D%E8%AA%AD%E3%82%93%E3%81%A0%EF%BC%81&url=https%3A%2F%2Fncode.syosetu.com%2Fn3025gy%2F&hashtags=narou%2CnarouN3025GY\" class=\"twitter-share-button\">Tweet</a>\n\n<script>!function(d,s,id){var js,fjs=d.getElementsByTagName(s)[0],p=/^http:/.test(d.location)?'http':'https';if(!d.getElementById(id)){js=d.createElement(s);js.id=id;js.src=p+'://platform.twitter.com/widgets.js';fjs.parentNode.insertBefore(js,fjs);}}(document, 'script', 'twitter-wjs');</script>\n\n</div>\n\n<div id=\"impression\">\n<div class=\"center\">\n感想を書く場合は<a href=\"https://syosetu.com/login/input/\">ログイン</a>してください。\n</div>\n</div><!-- impression -->\n\n\n\n\n\n<div id=\"novel_attention\">\n+注意+<br />\n<span class=\"attention\">特に記載なき場合、掲載されている作品はすべてフィクションであり実在の人物・団体等とは一切関係ありません。<br />\n特に記載なき場合、掲載されている作品の著作権は作者にあります(一部作品除く)。<br />\n作者以外の方による作品の引用を超える無断転載は禁止しており、行った場合、著作権法の違反となります。<br />\n</span><br />\nこの作品はリンクフリーです。ご自由にリンク(紹介)してください。<br />\nこの作品はスマートフォン対応です。スマートフォンかパソコンかを自動で判別し、適切なページを表示します。<br />\n作品の読了時間は毎分500文字を読むと想定した場合の時間です。目安にして下さい。\n</div><!--novel_attention-->\n\n\n<div id=\"novel_footer\">\n<ul class=\"undernavi\">\n<li><a href=\"https://mypage.syosetu.com/1527965/\">作者マイページ</a></li>\n\n\n<li><a href=\"https://novelcom.syosetu.com/novelreport/input/ncode/1802845/no/2/\">誤字報告</a></li>\n\n<li><a href=\"https://syosetu.com/ihantsuhou/input/ncode/1802845/\">情報提供</a></li>\n</ul>\n</div><!--novel_footer-->\n\n</div><!--novel_contents-->\n\n\n\n\n\n<div class=\"koukoku_300x2\">\n<div class=\"koukoku_300x2__ad\">\n\n<div id=\"dd1c8cc7850bd85f89ee2a18893bbf61\" >\n<script type=\"text/javascript\">\nmicroadCompass.queue.push({\n\"spot\": \"dd1c8cc7850bd85f89ee2a18893bbf61\"\n});\n</script>\n</div>\n\n\n</div><!-- /.koukoku_300x2__ad -->\n<div class=\"koukoku_300x2__ad\">\n\n<div id=\"a1ccae3ae3c6e917e252e5e1cd4aae13\" >\n<script type=\"text/javascript\">\nmicroadCompass.queue.push({\n\"spot\": \"a1ccae3ae3c6e917e252e5e1cd4aae13\"\n});\n</script>\n</div>\n\n\n</div><!-- /.koukoku_300x2__ad -->\n</div><!-- /.koukoku_300x2 -->\n\n\n\n\n\n\n<a id=\"pageTop\" class=\"pageTop pageTop--up\" href=\"#main\">\u2191ページトップへ</a>\n\n</div><!--container-->\n\n<!-- フッタここから -->\n<div id=\"footer\">\n<ul class=\"undernavi\">\n<li><a href=\"https://syosetu.com\">小説家になろう</a></li>\n<li><a href=\"https://yomou.syosetu.com\">小説を読もう！</a></li>\n<li id=\"search\">\n<form action=\"https://yomou.syosetu.com/search.php\">\n<input name=\"word\" size=\"21\" type=\"text\" />\n<input value=\"検索\" type=\"submit\" />\n</form>\n</li>\n</ul>\n</div><!--footer-->\n<!-- フッタここまで -->\n\n\n    <!-- Global site tag (gtag.js) - Google Analytics -->\n    <script async src=\"https://www.googletagmanager.com/gtag/js?id=G-1TH9CF4FPC\"></script>\n    <script>\n        window.dataLayer = window.dataLayer || [];\n        function gtag(){dataLayer.push(arguments);}\n        gtag('js', new Date());\n\n        gtag('config', 'G-1TH9CF4FPC');\n    </script>\n\n                <script>\n            gtag('config', 'G-2YQV7PZTL9');\n        </script>\n    \n\n\n<script type=\"text/javascript\" src=\"//d-cache.microad.jp/js/td_sn_access.js\"></script>\n<script type=\"text/javascript\">\n  microadTd.SN.start({})\n</script>\n\n\n\n\n</body></html>\n";
          
            /*
            var url = $"{baseUrl}{chapter}";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return new DownloadResults { StatusCode = response.StatusCode };
            }
            var pageContent = await response.Content.ReadAsStringAsync();
            */
            
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageContent);
            var chapterTitle = doc.DocumentNode.SelectSingleNode("//p[@class='novel_subtitle']")?.InnerText ?? "No Chapter Title";

            var bodyNode = doc.DocumentNode.SelectSingleNode("//div[@id='novel_honbun']");
            if (bodyNode == null)
            {
                return new DownloadResults { StatusCode = HttpStatusCode.NotFound };
            }
            foreach (var lineNode in bodyNode.ChildNodes)
            {
                if (lineNode is not { Name: "p" })
                {
                    continue;
                }
                Console.WriteLine(lineNode.InnerText);   
            }
            return new DownloadResults { StatusCode = HttpStatusCode.OK, ChapterTitle = chapterTitle, Content = pageContent };
        }

        
        static void HandleParseError(ParserResult<Options> results, IEnumerable<Error> errs)
        {
            var error = HelpText.AutoBuild(results);
            Console.WriteLine(error);
        }
    }
}