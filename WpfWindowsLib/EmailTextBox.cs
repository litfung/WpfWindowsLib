﻿/**************************************************************************************

WpfWindowsLib.EmailTextBox
==========================

TextBox accepting only email addresses and implementing ICheck. 
The user can only key in characters like 'a' .. 'z', '0' .. '9', '.', '-' and '_'. More 
characters would be legal, but some email servers might not accept them. If more characters 
should be allowed, make a new class using the code here and change OnPreviewTextInput().

Written in 2020 by Jürgpeter Huber 
Contact: PeterCode at Peterbox dot com

To the extent possible under law, the author(s) have dedicated all copyright and 
related and neighboring rights to this software to the public domain worldwide under
the Creative Commons 0 license (details see COPYING.txt file, see also
<http://creativecommons.org/publicdomain/zero/1.0/>). 

This software is distributed without any warranty. 
**************************************************************************************/

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Windows;
using System.Windows.Input;


namespace WpfWindowsLib {


  /// <summary>
  /// A TextBox accepting only email addresses with exactly one '@' separating local-part 
  /// and domain-part. Per default, the user can only key in characters like 
  /// 'A' .. 'Z', 'a' .. 'z', '0' .. '9', '.', '+', '-' and '_'. It is also possible to allow
  /// an extended or maximum ASCII set, blanks and finally also UTF8.<para/>
  /// Once the user has finished entering the address, he gets a warning when the address violates
  /// default rules (one '@' in the middle and at least one '.' in the domain-part. The validation
  /// can be changed to a Microsoft Regex email address validator.<para/>
  /// If EmailTextBox is placed in a Window inherited from CheckedWindow, it reports automatically 
  /// any value change.
  /// </summary>
  public class EmailTextBox: CheckedTextBox {

    //https://social.technet.microsoft.com/Forums/exchange/en-US/69f393aa-d555-4f8f-bb16-c636a129fc25/what-are-valid-and-invalid-email-address-characters
    //http://www.dominicsayers.com/isemail/

    #region Methods
    //      -------

    const string defaultAsciiSpecialChars = ".@-_+";


    public static string AsciiSpecialChars { get; set; } = defaultAsciiSpecialChars;


    /// <summary>
    /// Allows only the special characters ".@-_+" in an email address
    /// </summary>
    public static void SetDefaultAsciiSpecialChars() {
      AsciiSpecialChars = defaultAsciiSpecialChars;
    }


    /// <summary>
    /// Allows extended ASCII control characters ".@-_+!#$%&'*/=?^`{|}~" in an email address
    /// </summary>
    public static void SetExtendedAsciiSpecialChars() {
      AsciiSpecialChars = ".@-_+!#$%&'*/=?^`{|}~";
    }


    /// <summary>
    /// Allows extended ASCII and quoted control characters ".@-_+!#$%&'*/=?^`{|}~"(),:;<>[\]" in an email address
    /// </summary>
    public static void SetExtendedQuotedAsciiSpecialChars() {
      AsciiSpecialChars = ".@-_+!#$%&'*/=?^`{|}~\"(),:;<>[\\]";
    }


    /// <summary>
    /// is a blank ' ' allowed in the local-part of the email address ?
    /// </summary>
    public static bool IsBlankAllowed { get; set; } = false;


    /// <summary>
    /// is any character greater than UTF8 U+007F allowed ?
    /// </summary>
    public static bool IsInternationalCharSetAllowed { get; set; } = false;


    /// <summary>
    /// Validates if char is valid in the local-part of an email address
    /// </summary>
    public static Func<char, bool> IsValidEmailChar { get; set; } = IsValidEmailCharDefault;


    /// <summary>
    /// Validates if char is valid in the domain-part of an email address
    /// </summary>
    public static Func<char, bool> IsValidDnsChar { get; set; } = IsValidEmailDnsDefault;


    /// <summary>
    /// Validates complete email address
    /// </summary>
    public static Func<string, bool> IsValidEmail { get; set; } = IsValidEmailDefault;
    #endregion


    #region Initialisation
    //      --------------

    protected override void OnTextBoxInitialised() {
      //verify the values set in XAML
      if (Text.Length>0 && !IsValidEmail(Text)) {
        throw new Exception($"Error EmailTextBox: '{Text}' is not a valid email address).");
      }
    }


    /// <summary>
    /// Sets initial value from code behind. If isRequired is true, user needs to change the value before saving is possible. 
    /// If min or max are null, Min or Max value gets not changed.
    /// </summary>
    public override void Initialise(string? text = null, bool? isRequired = false) {

      if (!string.IsNullOrEmpty(text) && !IsValidEmail(text)) {
        throw new Exception($"Error EmailTextBox.Initialise(): '{text}' is not a valid email address).");
      }
      base.Initialise(text, isRequired);
    }
    #endregion


    #region Overrides
    //      ---------

    protected override void OnPreviewKeyDown(KeyEventArgs e) {
      if (!IsBlankAllowed) {
        e.Handled = e.Key == Key.Space;//forbid " " here, because it doesn't show up in OnPreviewTextInput
      }

      base.OnPreviewKeyDown(e);
    }


    protected override void OnPreviewTextInput(TextCompositionEventArgs e) {
      var isError = false;
      if (e.Text.Length>0) { //ctrl + key results in Text.Length==0
        if (e.Text.Length>1 || e.Text[0]>0x7F) {
          //character greater than biggest ASCII character
          if (!IsInternationalCharSetAllowed) {
            isError = true;
          }
        } else {
          var checkChar = e.Text[0];
          if (checkChar=='@') {
            if (CaretIndex==0) {
              isError = true;
            } else {
              var textWithoutSelection = Text[..CaretIndex] + Text[(CaretIndex + SelectionLength)..];
              isError = textWithoutSelection.Contains('@');
            }
          } else {
            isError = !IsValidEmailChar(checkChar);
          }
        }
      }
      e.Handled= isError;
      base.OnPreviewTextInput(e); 
    }


    protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e) {
      if (Text.Length>0) {
        if (!IsValidEmail(Text)) {
          var result = MessageBox.Show($"'{Text}' might not be a valid email address. Press'Ok' to accept email address as it is or press 'Cancel' " + 
            "to change the email address.", "Invalid Email Address ?", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
          if (result!=MessageBoxResult.OK) {
            e.Handled = true;
          }
        }
      }
      base.OnPreviewLostKeyboardFocus(e);
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Validates if char is valid in the local-part of an email address
    /// </summary>
    public static bool IsValidEmailCharDefault(char emailChar) {
      if ((emailChar>='a' && emailChar<='z') ||
        (emailChar>='A' && emailChar<='Z') ||
        (emailChar>='0' && emailChar<='9')) {
        return true;
      }
      foreach (var specialChar in AsciiSpecialChars) {
        if (emailChar==specialChar) return true;
      }
      if (IsBlankAllowed && emailChar==' ') return true;

      if (IsInternationalCharSetAllowed && emailChar>0x7F) return true;

      return false;
    }


    /// <summary>
    /// Validates if char is valid in the domain-part of an email address
    /// </summary>
    public static bool IsValidEmailDnsDefault(char emailChar) {
      return (emailChar>='a' && emailChar<='z') ||
        (emailChar>='A' && emailChar<='Z') ||
        (emailChar>='0' && emailChar<='9') ||
        (emailChar=='.') ||
        (emailChar=='-');
    }


    static readonly EmailAddressAttribute emailValidator = new EmailAddressAttribute();


    /// <summary>
    /// Validates if text is a valid email address. Uses a regex expression from Microsoft too complicated for
    /// me to understand:
    /// pattern = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$";
    /// it is also not 100% correct, for example it accepts '1@1.', which is probably not a valid email address because
    /// of the trailing '.'.
    /// </summary>
    public static bool IsValidEmailMicrosoft(string address) {
      return emailValidator.IsValid(address);
    }


    /// <summary>
    /// Simple test if email address local-part@domain-part follows some basic rules:<para/> 
    /// + has exactly one @ separating 2 strings<para/>
    /// + local-part is 1 to 64 characters long
    /// + has at least one dot in the domain-part<para/>
    /// + domain-part is 4 to 254-local-part.Lenght characters long
    /// + has at least four characters in the domain-part<para/>
    /// + has not 2 consecutive dots '.' in domain part
    /// + only legal characters
    /// </summary>
    static bool IsValidEmailDefault(string address) {
      //inspired by
      //https://softwareengineering.stackexchange.com/questions/78353/how-far-should-one-take-e-mail-address-validation/78363#78363
      //@ testing
      var atIndex = address.LastIndexOf("@");
      if (atIndex==0 || address.LastIndexOf(".")<=atIndex) return false;

      //local-part testing
      var addressSpan = address.AsSpan();
      var localPart = addressSpan[..atIndex];
      if (localPart.Length>64) return false;

      foreach (var addressChar in localPart) {
        if (!IsValidEmailChar(addressChar)) return false;
      }

      //domain-part testing
      var domainPart = addressSpan[(atIndex+1)..];
      if (domainPart.Length<4 || domainPart.Length>254-localPart.Length) return false;

      var isDot = false;
      foreach (var addressChar in domainPart) {
        if (!IsValidDnsChar(addressChar)) return false;
        if (isDot) {
          if (addressChar=='.') return false;

          isDot = false;
        } else {
          isDot = addressChar=='.';
        }
      }

      return true;
    }
    #endregion
  }
}
