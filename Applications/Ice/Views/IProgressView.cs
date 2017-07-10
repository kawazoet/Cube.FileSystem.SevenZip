﻿/* ------------------------------------------------------------------------- */
///
/// Copyright (c) 2010 CubeSoft, Inc.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///  http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
///
/* ------------------------------------------------------------------------- */
using System;
using System.Drawing;

namespace Cube.FileSystem.App.Ice
{
    /* --------------------------------------------------------------------- */
    ///
    /// IProgressView
    /// 
    /// <summary>
    /// 圧縮・展開の進捗状況を表示するインターフェースです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public interface IProgressView : Cube.Forms.IForm
    {
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Value
        ///
        /// <summary>
        /// 進捗状況を示す値をパーセント単位で取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        int Value { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Status
        ///
        /// <summary>
        /// 現在の状況を表す文字列を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        string Status { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// FileName
        ///
        /// <summary>
        /// 対象とするファイル名を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        string FileName { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// DoneCount
        ///
        /// <summary>
        /// 処理を終了したファイル数を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        long DoneCount { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// FileCount
        ///
        /// <summary>
        /// 処理対象ファイル数の合計を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        long FileCount { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Elapsed
        ///
        /// <summary>
        /// 圧縮・展開処理開始からの経過時間を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        TimeSpan Elapsed { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Icon
        ///
        /// <summary>
        /// アイコンを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        Icon Icon { get; set; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Start
        ///
        /// <summary>
        /// タイマーを開始します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        void Start();

        /* ----------------------------------------------------------------- */
        ///
        /// Stop
        ///
        /// <summary>
        /// タイマーを停止します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        void Stop();

        #endregion
    }
}
