﻿using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text;
using Metroit.Windows.Forms.Extensions;
using Metroit.Windows.Forms.ComponentModel;

namespace Metroit.Windows.Forms
{
    /// <summary>
    /// 入力文字種、入力バイト数制限機能を設けたテキストエリアを提供します。
    /// </summary>
    /// <remarks>
    /// テキストボックスとしての機能をいくつか拡張し、利用することができます。<br />
    /// [拡張機能]<br />
    /// 　・Bytes指定による入力Bytes数制限。<br />
    /// 　・入力許可文字制限。<br />
    /// <br />
    /// 入力許可文字制限は、全て許可、半角数字、全角数字、半角英字、全角英字、半角記号、全角記号、カスタムを組み合わせて利用することができます。<br />
    /// 入力許可文字制限に、改行コードは含まれません。<br />
    /// 全て許可、カスタムを指定した場合は、他のどの文字制限も併せて利用することはできません。<br />
    /// 許可文字内から、拒否する文字を指定することができます。<br />
    /// カスタムを設定した場合、許可する文字を指定することができます。<br />
    /// </remarks>
    [ToolboxItem(true)]
    public class MetLimitedTextBox : MetTextBox
    {
        /// <summary>
        /// 各入力値検証を行った状態
        /// </summary>
        private enum CharacterValidationStatus
        {
            /// <summary>
            /// 入力値を許可する。
            /// </summary>
            Allow,
            /// <summary>
            /// 入力値を拒否する。
            /// </summary>
            Deny,
            /// <summary>
            /// 入力値に合致するパターンでないか、特定パターンでは拒否される値である。
            /// </summary>
            NoControl
        }

        /// <summary>
        /// MetLimitedTextBox の新しいインスタンスを初期化します。
        /// </summary>
        public MetLimitedTextBox() : base()
        {
            // イベントハンドラの追加
            this.TextChangeValidation += MetLimitedTextBox_TextChangeValidation;
        }

        #region イベント

        /// <summary>
        /// テキスト変更中、バイト数チェック、許可文字チェックを行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">入力値許可イベントオブジェクト。</param>
        private void MetLimitedTextBox_TextChangeValidation(object sender, TextChangeValidationEventArgs e)
        {
            int afterByteCount = this.ByteEncoding.GetByteCount(e.After);

            // 最大バイト長制御
            if (this.MaxByteLength > 0 && afterByteCount > this.MaxByteLength)
            {
                e.Cancel = true;
                return;
            }

            // 複数行許可の場合は改行コードを除去した文字列をチェック対象とする
            string checkValue = e.Input.ToString();
            if (Multiline)
            {
                checkValue = checkValue.Replace(Environment.NewLine, "");
            }

            // 許可文字チェック
            if (!this.isValidChars(checkValue))
            {
                e.Cancel = true;
                return;
            }
        }

        #endregion

        #region メソッド

        /// <summary>
        /// バイト数を考慮して、オートフォーカスを行うか確認します。
        /// </summary>
        /// <returns>true:オートフォーカス可, false:オートフォーカス不可</returns>
        protected override bool CanAutoFocus()
        {
            if (!base.CanAutoFocus())
            {
                if (this.MaxByteLength == 0)
                {
                    return false;
                }

                int afterTextByte = this.ByteEncoding.GetByteCount(this.Text);
                if (afterTextByte == this.MaxByteLength)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 入力された文字列を1文字ずつ有効文字かチェックします。
        /// </summary>
        /// <param name="inputText">入力文字列</param>
        /// <returns>true:有効, false:無効</returns>
        private bool isValidChars(string inputText)
        {
            CharacterValidationStatus result;
            foreach (var singleChar in inputText.ToString())
            {
                // すべて許可
                result = this.validateAllChar(singleChar);
                if (result == CharacterValidationStatus.Allow)
                {
                    continue;
                }
                if (result == CharacterValidationStatus.Deny)
                {
                    return false;
                }

                // カスタム
                result = this.validateCustomChar(singleChar);
                if (result == CharacterValidationStatus.Allow)
                {
                    continue;
                }
                if (result == CharacterValidationStatus.Deny)
                {
                    return false;
                }

                // 半角数値
                result = this.validateHalfNumericChar(singleChar);
                if (result == CharacterValidationStatus.Allow)
                {
                    continue;
                }
                if (result == CharacterValidationStatus.Deny)
                {
                    return false;
                }

                // 全角数値
                result = this.validateFullNumericChar(singleChar);
                if (result == CharacterValidationStatus.Allow)
                {
                    continue;
                }
                if (result == CharacterValidationStatus.Deny)
                {
                    return false;
                }

                // 半角英字
                result = this.validateHalfAlphaChar(singleChar);
                if (result == CharacterValidationStatus.Allow)
                {
                    continue;
                }
                if (result == CharacterValidationStatus.Deny)
                {
                    return false;
                }

                // 全角英字
                result = this.validateFullAlphaChar(singleChar);
                if (result == CharacterValidationStatus.Allow)
                {
                    continue;
                }
                if (result == CharacterValidationStatus.Deny)
                {
                    return false;
                }

                // 半角記号
                result = this.validateHalfSignChar(singleChar);
                if (result == CharacterValidationStatus.Allow)
                {
                    continue;
                }
                if (result == CharacterValidationStatus.Deny)
                {
                    return false;
                }

                // 全角記号
                result = this.validateFullSignChar(singleChar);
                if (result == CharacterValidationStatus.Allow)
                {
                    continue;
                }
                if (result == CharacterValidationStatus.Deny)
                {
                    return false;
                }

                // どのパターンにも合致しない場合
                return false;
            }

            return true;
        }

        /// <summary>
        /// 対象文字が拒否文字かどうかを取得する。
        /// </summary>
        /// <param name="singleChar">文字</param>
        /// <returns>true:拒否文字, false:拒否文字でない</returns>
        private bool isExcludeChar(char singleChar)
        {
            if (Array.IndexOf<string>(this.ExcludeChars, singleChar.ToString()) >= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 対象文字が個別許可した全角記号であるかどうかを取得する。
        /// </summary>
        /// <param name="singleChar">文字</param>
        /// <returns>true:個別許可した全角記号, false:個別許可した全角記号でない</returns>
        private bool isFullSignSpecialChar(char singleChar)
        {
            if (Array.LastIndexOf<string>(this.FullSignSpecialChars, singleChar.ToString()) < 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 対象文字が全て許可による許可文字か検証する。
        /// </summary>
        /// <param name="singleChar">文字</param>
        /// <returns>検証結果</returns>
        private CharacterValidationStatus validateAllChar(char singleChar)
        {
            if (!(this.AcceptsChar == AcceptsCharType.All))
            {
                return CharacterValidationStatus.NoControl;
            }

            if (this.isExcludeChar(singleChar))
            {
                return CharacterValidationStatus.Deny;
            }

            return CharacterValidationStatus.Allow;
        }

        /// <summary>
        /// 対象文字がカスタムによる許可文字か検証する。
        /// </summary>
        /// <param name="singleChar">文字</param>
        /// <returns>検証結果</returns>
        private CharacterValidationStatus validateCustomChar(char singleChar)
        {
            if (!(this.AcceptsChar == AcceptsCharType.Custom))
            {
                return CharacterValidationStatus.NoControl;
            }

            if (Array.LastIndexOf<string>(this.CustomChars, singleChar.ToString()) < 0)
            {
                return CharacterValidationStatus.Deny;
            }

            if (this.isExcludeChar(singleChar))
            {
                return CharacterValidationStatus.Deny;
            }

            return CharacterValidationStatus.Allow;
        }

        /// <summary>
        /// 対象文字が半角数値による許可文字か検証する。
        /// </summary>
        /// <param name="singleChar">文字</param>
        /// <returns>検証結果</returns>
        private CharacterValidationStatus validateHalfNumericChar(char singleChar)
        {
            if (!((this.AcceptsChar & AcceptsCharType.HalfNumeric) == AcceptsCharType.HalfNumeric))
            {
                return CharacterValidationStatus.NoControl;
            }

            if (!this.isHalfNumeric(singleChar))
            {
                return CharacterValidationStatus.NoControl;
            }

            if (this.isExcludeChar(singleChar))
            {
                return CharacterValidationStatus.Deny;
            }

            return CharacterValidationStatus.Allow;
        }

        /// <summary>
        /// 対象文字が全角数値による許可文字か検証する。
        /// </summary>
        /// <param name="singleChar">文字</param>
        /// <returns>検証結果</returns>
        private CharacterValidationStatus validateFullNumericChar(char singleChar)
        {
            if (!((this.AcceptsChar & AcceptsCharType.FullNumeric) == AcceptsCharType.FullNumeric))
            {
                return CharacterValidationStatus.NoControl;
            }

            if (!this.isFullNumeric(singleChar))
            {
                return CharacterValidationStatus.NoControl;
            }

            if (this.isExcludeChar(singleChar))
            {
                return CharacterValidationStatus.Deny;
            }

            return CharacterValidationStatus.Allow;
        }

        /// <summary>
        /// 対象文字が半角英字による許可文字か検証する。
        /// </summary>
        /// <param name="singleChar">文字</param>
        /// <returns>検証結果</returns>
        private CharacterValidationStatus validateHalfAlphaChar(char singleChar)
        {
            if (!((this.AcceptsChar & AcceptsCharType.HalfAlpha) == AcceptsCharType.HalfAlpha))
            {
                return CharacterValidationStatus.NoControl;
            }

            if (!this.isHalfAlpha(singleChar))
            {
                return CharacterValidationStatus.NoControl;
            }

            if (this.isExcludeChar(singleChar))
            {
                return CharacterValidationStatus.Deny;
            }

            return CharacterValidationStatus.Allow;
        }

        /// <summary>
        /// 対象文字が全角英字による許可文字か検証する。
        /// </summary>
        /// <param name="singleChar">文字</param>
        /// <returns>検証結果</returns>
        private CharacterValidationStatus validateFullAlphaChar(char singleChar)
        {
            if (!((this.AcceptsChar & AcceptsCharType.FullAlpha) == AcceptsCharType.FullAlpha))
            {
                return CharacterValidationStatus.NoControl;
            }

            if (!this.isFullAlpha(singleChar))
            {
                return CharacterValidationStatus.NoControl;
            }

            if (this.isExcludeChar(singleChar))
            {
                return CharacterValidationStatus.Deny;
            }

            return CharacterValidationStatus.Allow;
        }

        /// <summary>
        /// 対象文字が半角記号による許可文字か検証する。
        /// </summary>
        /// <param name="singleChar">文字</param>
        /// <returns>検証結果</returns>
        private CharacterValidationStatus validateHalfSignChar(char singleChar)
        {
            if (!((this.AcceptsChar & AcceptsCharType.HalfSign) == AcceptsCharType.HalfSign))
            {
                return CharacterValidationStatus.NoControl;
            }

            if (!this.isHalfSign(singleChar))
            {
                return CharacterValidationStatus.NoControl;
            }

            if (this.isExcludeChar(singleChar))
            {
                return CharacterValidationStatus.Deny;
            }

            return CharacterValidationStatus.Allow;
        }

        /// <summary>
        /// 対象文字が全角記号による許可文字か検証する。
        /// </summary>
        /// <param name="singleChar">文字</param>
        /// <returns>検証結果</returns>
        private CharacterValidationStatus validateFullSignChar(char singleChar)
        {
            if (!((this.AcceptsChar & AcceptsCharType.FullSign) == AcceptsCharType.FullSign))
            {
                return CharacterValidationStatus.NoControl;
            }

            if (!this.isFullSign(singleChar) && !this.isFullSignSpecialChar(singleChar))
            {
                return CharacterValidationStatus.NoControl;
            }

            if (this.isExcludeChar(singleChar))
            {
                return CharacterValidationStatus.Deny;
            }

            return CharacterValidationStatus.Allow;
        }

        /// <summary>
        /// 1文字分の入力値が半角数字かどうかを取得する。
        /// </summary>
        /// <param name="value">1文字分の入力値。</param>
        /// <returns>true:半角数字, false:半角数字でない</returns>
        private bool isHalfNumeric(char value)
        {
            if ((int)value >= (int)'0' && (int)value <= (int)'9')
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 1文字分の入力値が全角数字かどうかを取得する。
        /// </summary>
        /// <param name="value">1文字分の入力値。</param>
        /// <returns>true:全角数字, false:全角数字でない</returns>
        private bool isFullNumeric(char value)
        {
            if ((int)value >= (int)'０' && (int)value <= (int)'９')
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 1文字分の入力値が半角英字かどうかを取得する。
        /// </summary>
        /// <param name="value">1文字分の入力値。</param>
        /// <returns>true:半角英字, false:半角英字でない</returns>
        private bool isHalfAlpha(char value)
        {
            if (((int)value >= (int)'A' && (int)value <= (int)'Z') ||
                    ((int)value >= (int)'a' && (int)value <= (int)'z'))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 1文字分の入力値が全角英字かどうかを取得する。
        /// </summary>
        /// <param name="value">1文字分の入力値。</param>
        /// <returns>true:全角英字, false:全角英字でない</returns>
        private bool isFullAlpha(char value)
        {
            if (((int)value >= (int)'Ａ' && (int)value <= (int)'Ｚ') ||
                    ((int)value >= (int)'ａ' && (int)value <= (int)'ｚ'))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 1文字分の入力値が半角記号かどうかを取得する。
        /// </summary>
        /// <param name="value">1文字分の入力値。</param>
        /// <returns>true:半角記号, false:半角記号でない</returns>
        private bool isHalfSign(char value)
        {
            switch ((int)value)
            {
                case (int)' ':
                case (int)'!':
                case (int)'"':
                case (int)'#':
                case (int)'$':
                case (int)'%':
                case (int)'&':
                case (int)'\'':
                case (int)'(':
                case (int)')':
                case (int)'-':
                case (int)'=':
                case (int)'^':
                case (int)'~':
                case (int)'\\':
                case (int)'|':
                case (int)'@':
                case (int)'`':
                case (int)'[':
                case (int)'{':
                case (int)';':
                case (int)'+':
                case (int)':':
                case (int)'*':
                case (int)']':
                case (int)'}':
                case (int)',':
                case (int)'<':
                case (int)'.':
                case (int)'>':
                case (int)'/':
                case (int)'?':
                case (int)'_':
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 1文字分の入力値が全角記号かどうかを取得する。
        /// </summary>
        /// <param name="value">1文字分の入力値。</param>
        /// <returns>true:全角記号, false:全角記号でない</returns>
        private bool isFullSign(char value)
        {
            switch ((int)value)
            {
                case (int)'　':
                case (int)'！':
                case (int)'”':
                case (int)'＃':
                case (int)'＄':
                case (int)'％':
                case (int)'＆':
                case (int)'’':
                case (int)'（':
                case (int)'）':
                case (int)'－':
                case (int)'＝':
                case (int)'＾':
                case (int)'～':
                case (int)'￥':
                case (int)'｜':
                case (int)'＠':
                case (int)'‘':
                case (int)'「':
                case (int)'｛':
                case (int)'；':
                case (int)'＋':
                case (int)'：':
                case (int)'＊':
                case (int)'」':
                case (int)'｝':
                case (int)'，':
                case (int)'＜':
                case (int)'．':
                case (int)'＞':
                case (int)'・':
                case (int)'／':
                case (int)'？':
                case (int)'＿':

                    return true;
            }

            return false;
        }

        #endregion

        #region 追加プロパティ

        /// <summary>
        /// エディット コントロールに入力できる最大バイト数を指定します。0を指定した場合、無制限となります。
        /// </summary>
        /// <remarks>
        /// 0を指定した場合、無制限となります。<br />
        /// 半角1バイト、全角2バイトとして制限されます。<br />
        /// 当プロパティを設定することによってMaxLengthが無視されるわけではありません。
        /// </remarks>
        [Browsable(true)]
        [MetCategory("MetBehavior")]
        [DefaultValue(typeof(uint), "0")]
        [MetDescription("ControlMaxByteLength")]
        public uint MaxByteLength { get; set; } = 0;

        /// <summary>
        /// 最大バイト数制限を実施する文字エンコーディングを指定します。
        /// </summary>
        [Browsable(true)]
        [MetCategory("MetBehavior")]
        [TypeConverter(typeof(EncodingNameConverter))]
        [MetDescription("ControlByteEncoding")]
        public Encoding ByteEncoding { get; set; } = Encoding.Default;

        /// <summary>
        /// ByteEncoding が既定値から変更されたかどうかを返却する。
        /// </summary>
        /// <returns>true:変更された, false:変更されていない</returns>
        private bool ShouldSerializeByteEncoding() => this.ByteEncoding != Encoding.Default;

        /// <summary>
        /// ByteEncoding のリセット操作を行う。
        /// </summary>
        private void ResetByteEncoding() => this.ByteEncoding = Encoding.Default;

        /// <summary>
        /// 入力が許可される文字の種類を設定します。
        /// </summary>
        /// <remarks>
        /// 当プロパティは、アプリケーションの状態に応じて変化させるアプローチを可能とする為、コード上で変更可能としています。<br />
        /// コード上でText値が設定済みの際に変更する場合は気を付けてください。
        /// </remarks>
        [Browsable(true)]
        [MetCategory("MetBehavior")]
        [MetDescription("ControlAcceptsChar")]
        [Editor(typeof(ComponentModel.AcceptsCharEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue(AcceptsCharType.All)]
        public AcceptsCharType AcceptsChar { get; set; } = AcceptsCharType.All;

        /// <summary>
        /// 入力許可文字の種類がカスタムの場合に、入力を許可する単一文字を1行単位で設定します。
        /// </summary>
        /// <remarks>
        /// ApprovalCharactersがカスタムの場合のみ有効です。
        /// 当プロパティは、アプリケーションの状態に応じて変化させるアプローチを可能とする為、コード上で変更可能としています。<br />
        /// コード上でText値が設定済みの際に変更する場合は気を付けてください。
        /// </remarks>
        [Browsable(true)]
        [MetCategory("MetBehavior")]
        [MetDescription("ControlCustomChars")]
        [Editor("System.Windows.Forms.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor))]
        public string[] CustomChars { get; set; } = new string[] { };

        /// <summary>
        /// 個別で許可する全角記号を1行単位で設定します。
        /// </summary>
        [Browsable(true)]
        [MetCategory("MetBehavior")]
        [MetDescription("ControlFullSignSpecialChars")]
        [Editor("System.Windows.Forms.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor))]
        public string[] FullSignSpecialChars { get; set; } = new string[] { };

        /// <summary>
        /// 入力許可文字の種類がカスタム以外の場合に、入力を拒否する単一文字を1行単位で設定します。
        /// </summary>
        /// <remarks>
        /// ApprovalCharactersがカスタム以外の場合のみ有効です。<br />
        /// 当プロパティは、アプリケーションの状態に応じて変化させるアプローチを可能とする為、コード上で変更可能としています。<br />
        /// コード上でText値が設定済みの際に変更する場合は気を付けてください。
        /// </remarks>
        [Browsable(true)]
        [MetCategory("MetBehavior")]
        [MetDescription("ControlExcludeChars")]
        [Editor("System.Windows.Forms.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor))]
        public string[] ExcludeChars { get; set; } = new string[] { };

        #endregion
    }
}
