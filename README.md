# waifu2x_caffe_multilang_gui
Multilingual GUI for waifu2x-caffe. Localization can be done via user-editable xaml file

## Localization
1. The localization files have the name UILang._language-code_.xaml; where _language-code_ is a 5-character identifier like en-US, zh-TW, ja-JP.
2. Make a copy of one of the bundled localization files.
3. Rename the file with your target language code replacing the original.
4. Using a text editor that support UTF-8, make the following changes:
  * ```<sys:String x:Key="ResourceDictionaryName">waifu2xui-en-US</sys:String>```
  * replace en-US with the target language code
  * All the text enclosed by the ```<sys:String>``` tags
5. About language code:
  * make up from _ab_-_XY_
  * ab can be found [http://www.loc.gov/standards/iso639-2/php/langcodes-search.php](Here) as Alpha-2 codes
  * XY can be found [https://www.iso.org/obp/ui/#search](Here)
  * Essentially ab is the language, XY is the country
## Points-to-note
* You need a working copy of waifu2x-caffe
* Uzip all files directly to the base folder of waifu2x-caffe, do not put it in into subdirectory.
* Do not rename the default subdirectory (i.e. there should be a "models" folder)
* Do not delete or move _waifu2x-caffe-cui.exe_

## License and Sharing
* Do what the fuck you want with this soft
* No warranty attached
* Share your localization on the ISSUE page first, or post it into the waifu2x thread on videohelp.com forum
* Feature request: I do not accept any feature request unless getting paid
