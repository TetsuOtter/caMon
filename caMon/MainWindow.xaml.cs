﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

using TR.BIDSSMemLib;

namespace caMon
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : NavigationWindow
	{
    SMemLib SML;

    /// <summary>ページセレクタのインスタンス.  起動中は不変</summary>
    readonly ISelector Selector_inst = null;

    IPages __ShowingPage = null;
    IPages ShowingPage
		{
      get => __ShowingPage;
			set
			{
        //NULLは許容されない
        if (value == null)
          throw new Exception("Setting the null value is not allowed.");

        //新ページの設定
        value.BackToHome += OnBackToHome;
        value.CloseApp += OnCloseAppFired;
        NavigationService.Navigate(value.FrontPage);

        //旧ページの解放
				if (__ShowingPage != null)
				{
          __ShowingPage.BackToHome -= OnBackToHome;
          __ShowingPage.CloseApp -= OnCloseAppFired;
          __ShowingPage.Dispose();
				}

        __ShowingPage = value;//表示中ページ記録の更新
			}
		}

		public MainWindow()
		{
			InitializeComponent();

      SharedFuncs.SMem_RStart();

      Selector_inst = new selector.default_.SelectPage();

			Selector_inst.PageChangeRequest += Selector_inst_PageChangeRequest;

      ShowingPage = Selector_inst;
		}

		private void Selector_inst_PageChangeRequest(object sender, PageChangeEventArgs e)
		{
      if (e.NewPage != null)
        ShowingPage = e.NewPage;
      else if (!string.IsNullOrWhiteSpace(e.ModPath))
        try
        {
          //pathからmodをload
          ShowingPage = ModLoader.LoadDllInst<IPages>(e.ModPath);
        }
        catch (FileNotFoundException fnfe)
        {
          MessageBox.Show("指定のmodファイルが見つかりませんでした\n" + fnfe.ToString());
          return;
        }
        catch (EntryPointNotFoundException epnfe)
        {
          MessageBox.Show("指定のmodファイルにページ実装が含まれていませんでした\n" + epnfe.ToString());
        }
        catch (Exception ex)
        {
          MessageBox.Show("不明なエラーが発生しました.\n" + ex.ToString());
        }
      else
        MessageBox.Show("次のページの指定がされていません");
    }

    private void OnCloseAppFired(object sender, EventArgs e) => this.Close();

    private void OnBackToHome(object sender, EventArgs e) => ShowingPage = Selector_inst;

    private void MainWindowHeadder_PreviewKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.F11)
      {
        switch (MainWindowHeadder.WindowState)
        {
          case WindowState.Maximized:
            MainWindowHeadder.WindowState = WindowState.Normal;
            MainWindowHeadder.WindowStyle = WindowStyle.SingleBorderWindow;
            break;
          case WindowState.Normal:
            MainWindowHeadder.WindowStyle = WindowStyle.None;
            MainWindowHeadder.WindowState = WindowState.Maximized;
            break;
        }
      }
      if (e.Key == Key.F12)
      {
        switch (MainWindowHeadder.WindowStyle)
        {
          case WindowStyle.None:
            MainWindowHeadder.WindowStyle = WindowStyle.SingleBorderWindow;
            break;
          case WindowStyle.SingleBorderWindow:
            MainWindowHeadder.WindowStyle = WindowStyle.None;
            break;
        }
      }
    }

		private void MainWindowHeadder_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
      SharedFuncs.SMem_RStop();
		}
	}
}
