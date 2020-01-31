﻿/**************************************************************************************

Samples.DecimalTextBoxWindow
============================

Sample Window for DecimalTextBox testing

Written in 2020 by Jürgpeter Huber 
Contact: PeterCode at Peterbox dot com

To the extent possible under law, the author(s) have dedicated all copyright and 
related and neighboring rights to this software to the public domain worldwide under
the Creative Commons 0 license (details see COPYING.txt file, see also
<http://creativecommons.org/publicdomain/zero/1.0/>). 

This software is distributed without any warranty. 
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using WpfWindowsLib;


namespace Samples {


  /// <summary>
  /// Interaction logic for DecimalTextBoxWindow.xaml
  /// </summary>
  public partial class DecimalTextBoxWindow: CheckedWindow {

    #region Constructor
    //      -----------

    public DecimalTextBoxWindow() {
      InitializeComponent();

      InitialiseButton.Click += initialiseButton_Click;
      SaveButton.Click += saveButton_Click;
      updateSaveButtonIsEnabled();
    }


    //private void MinTextBox_TextChanged(object sender, TextChangedEventArgs e) {
    //  if (int.TryParse(MinTextBox.Text, out int result)) {
    //    TestDecimalTextBox.Min = result;
    //  } else {
    //    TestDecimalTextBox.Min = int.MinValue;
    //  }
    //}
    #endregion


    #region Events
    //      ------

    private void initialiseButton_Click(object sender, RoutedEventArgs e) {
      try {
        TestDecimalTextBox.Initialise(
          InitialiseDecimalValueTextBox.Text.Length==0 ? (decimal?)null : decimal.Parse(InitialiseDecimalValueTextBox.Text),
          InitialisationIsRequriedCheckBox.IsChecked,
          InitialiseMinTextBox.Text.Length==0 ? (decimal?)null : int.Parse(InitialiseMinTextBox.Text),
          InitialiseMaxTextBox.Text.Length==0 ? (decimal?)null : int.Parse(InitialiseMaxTextBox.Text));
      } catch (Exception ex) {
        MessageBox.Show(ex.Message, "Exception occurred", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void saveButton_Click(object sender, RoutedEventArgs e) {
      this.ResetHasChanged();
      updateSaveButtonIsEnabled();
      MessageBox.Show("Data would now be saved.", "Save data", MessageBoxButton.OK, MessageBoxImage.Information);
      //Signal that data has been saved. User needs to change something before saving is possible again
    }


    private void updateSaveButtonIsEnabled() {
      if (SaveButton!=null) {
        SaveButton.IsEnabled = HasICheckChanged && IsAvailable;
      }
    }
    #endregion


    #region Overrides
    //      ---------
    protected override void OnICheckChanged() {
      updateSaveButtonIsEnabled();
    }


    protected override void OnIsAvailableChanged() {
      updateSaveButtonIsEnabled();
    }
    #endregion
  }
}
