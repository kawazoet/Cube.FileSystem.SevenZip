﻿/* ------------------------------------------------------------------------- */
//
// Copyright (c) 2010 CubeSoft, Inc.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/* ------------------------------------------------------------------------- */
using System;
using System.Linq;
using Cube.FileSystem.SevenZip.Ice;
using Cube.Log;

namespace Cube.FileSystem.SevenZip.App.Ice
{
    /* --------------------------------------------------------------------- */
    ///
    /// ArchiveFacade
    ///
    /// <summary>
    /// 圧縮処理を実行するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public sealed class ArchiveFacade : ProgressFacade
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// ArchiveFacade
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="request">コマンドライン</param>
        /// <param name="settings">ユーザ設定</param>
        ///
        /* ----------------------------------------------------------------- */
        public ArchiveFacade(Request request, SettingsFolder settings)
            : base(request, settings) { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Details
        /// 
        /// <summary>
        /// 圧縮処理の実行時詳細設定を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ArchiveDetails Details { get; private set; }

        #endregion

        #region Events

        #region DetailsRequested

        /* ----------------------------------------------------------------- */
        ///
        /// DetailsRequested
        /// 
        /// <summary>
        /// 圧縮の詳細設定が要求された時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event QueryEventHandler<string, ArchiveDetails> DetailsRequested;

        /* ----------------------------------------------------------------- */
        ///
        /// RaiseDetailsRequested
        /// 
        /// <summary>
        /// DetailsRequested イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void RaiseDetailsRequested()
        {
            var info = IO.Get(Request.Sources.First());
            var path = IO.Combine(info.DirectoryName, $"{info.NameWithoutExtension}.zip");

            var e = new QueryEventArgs<string, ArchiveDetails>(path, true);
            DetailsRequested?.Invoke(this, e);
            if (e.Cancel) throw new OperationCanceledException();

            Details = e.Result;
        }

        #endregion

        #region MailRequired

        /* ----------------------------------------------------------------- */
        ///
        /// MailRequested
        /// 
        /// <summary>
        /// メール画面の表示が要求された時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event ValueEventHandler<string> MailRequested;

        /* ----------------------------------------------------------------- */
        ///
        /// RaiseMailRequested
        /// 
        /// <summary>
        /// MailRequested イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void RaiseMailRequested()
        {
            if (!Request.Mail) return;
            MailRequested?.Invoke(this, ValueEventArgs.Create(Destination));
        }

        #endregion

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Start
        /// 
        /// <summary>
        /// 圧縮を開始します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public override void Start()
        {
            try
            {
                Archive();
                ProgressResult();
                RaiseMailRequested();
                Open(Destination, Settings.Value.Archive.OpenDirectory);
            }
            catch (OperationCanceledException) { /* user cancel */ }
            catch (Exception err) { Error(err); }
            finally { ProgressStop(); }
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Archive
        /// 
        /// <summary>
        /// 圧縮処理を実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Archive()
        {
            var fmt   = GetFormat();
            var dest  = GetDestination(fmt);
            var query = !string.IsNullOrEmpty(Details.Password) || Request.Password ?
                        new Query<string, string>(x => RaisePasswordRequested(x)) :
                        null;

            System.Diagnostics.Debug.Assert(Details != null);
            this.LogDebug(string.Format("Format:{0}\tMethod:{1}", fmt, Details.CompressionMethod));

            using (var writer = new ArchiveWriter(fmt, IO))
            {
                writer.Option = Details.ToOption(Settings);
                if (Settings.Value.Archive.Filtering) writer.Filters = Settings.Value.GetFilters();
                foreach (var item in Request.Sources) writer.Add(item);
                ProgressStart();
                writer.Save(dest, query, CreateInnerProgress(x => Report = x));
            }

            // Move
            if (string.IsNullOrEmpty(Tmp) || !IO.Exists(Tmp)) return;
            IO.Move(Tmp, Destination, true);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetFormat
        /// 
        /// <summary>
        /// 圧縮フォーマットを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private Format GetFormat()
        {
            var f = Request.Format;

            switch (f)
            {
                case Format.Tar:
                case Format.Zip:
                case Format.SevenZip:
                case Format.Sfx:
                    Details = new ArchiveDetails(f);
                    break;
                case Format.BZip2:
                case Format.GZip:
                case Format.XZ:
                    Details = new ArchiveDetails(Format.Tar);
                    Details.CompressionMethod = f.ToMethod();
                    break;
                default:
                    RaiseDetailsRequested();
                    break;
            }

            return Details.Format;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetDestination
        /// 
        /// <summary>
        /// 保存先パスを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private string GetDestination(Format format)
        {
            if (!string.IsNullOrEmpty(Details?.Path)) Destination = Details.Path;
            else
            {
                var query   = Request.Sources.First();
                var user    = Request.Format;
                var kv      = GetSaveLocation(Settings.Value.Archive, Request.Format, query);
                var runtime = kv.Key == SaveLocation.Runtime;
                var path    = runtime ? kv.Value : AddFileName(kv.Value, format);

                if (!runtime && IO.Exists(path))
                {
                    var e = new PathQueryEventArgs(query, Request.Format, true);
                    OnDestinationRequested(e);
                    if (e.Cancel) throw new OperationCanceledException();
                    path = e.Result;
                }

                Destination = path;
            }

            var info = IO.Get(Destination);
            SetTmp(info.DirectoryName);
            return Tmp;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AddFileName
        /// 
        /// <summary>
        /// FileName を結合します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private string AddFileName(string src, Format format)
        {
            var name = IO.Get(Request.Sources.First()).NameWithoutExtension;
            var head = format.ToExtension();
            var tail = Details.CompressionMethod.ToExtension();
            var ext  = $"{head}{tail}";

            return IO.Combine(src, $"{name}{ext}");
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RaisePasswordRequested
        /// 
        /// <summary>
        /// 必要に応じて PasswordRequested イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void RaisePasswordRequested(QueryEventArgs<string, string> e)
        {
            if (!string.IsNullOrEmpty(Details.Password))
            {
                e.Result = Details.Password;
                e.Cancel = false;
            }
            else OnPasswordRequested(e);
        }

        #endregion
    }
}
