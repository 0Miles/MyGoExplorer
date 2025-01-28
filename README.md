# PowerToys Run: MyGo Explorer
一款 PowerToys Run 插件，用於快速搜尋 MyGo 相關台詞並下載圖片至剪貼簿  
![demo](docs/mygoexplorer.gif)

## 安裝
1. 下載已建置的擴充元件 zip 檔後解壓縮：  
   [GitHub Releases](https://github.com/0miles/MyGoExplorer/releases)
2. 關閉 PowerToys
3. 將解壓縮後的 `Community.PowerToys.Run.Plugin.MyGoExplorer` 資料夾放入：
   ```
   %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\
   ```
4. 開啟 PowerToys


## 使用方法
1. 開啟 PowerToys Run (`Alt + Space`)
2. 輸入：  
   ```
   mygo: <關鍵字>
   ```
3. 選擇結果
4. 圖片將自動下載並複製到剪貼簿
5. 直接貼上 (`Ctrl + V`) 即可使用圖片


## 暫存機制
- 下載的圖片將存儲於：  
  ```
  %TMP%\MyGoExplorer\
  ```
- 如果圖片已存在，則不會重新下載

## Credits
- 使用了 hydra00400 的台詞資料庫與 anon-tokyo API  
https://forum.gamer.com.tw/C.php?bsn=60076&snA=8289364
