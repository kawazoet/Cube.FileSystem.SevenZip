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
using System.Linq;
using Cube.FileSystem.SevenZip;
using Cube.FileSystem.Ice;
using Cube.Log;

namespace Cube.FileSystem.App.Ice
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
    public class ArchiveFacade : ProgressFacade
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
        /// Runtime
        /// 
        /// <summary>
        /// 圧縮処理の実行時詳細設定を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ArchiveRuntimeSettings Runtime { get; private set; }

        #endregion

        #region Events

        #region RuntimeSettingsRequired

        /* ----------------------------------------------------------------- */
        ///
        /// RuntimeSettingsRequired
        /// 
        /// <summary>
        /// 圧縮の詳細設定が要求された時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event QueryEventHandler<string, ArchiveRuntimeSettings> RuntimeSettingsRequired;

        /* ----------------------------------------------------------------- */
        ///
        /// OnRuntimeSettingsRequired
        /// 
        /// <summary>
        /// RuntimeSettingsRequired イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnRuntimeSettingsRequired(QueryEventArgs<string, ArchiveRuntimeSettings> e)
        {
            if (RuntimeSettingsRequired != null) RuntimeSettingsRequired(this, e);
            else e.Cancel = true;
            if (e.Cancel) throw new UserCancelException();
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
                Open(Destination, Settings.Value.Archive.OpenDirectory);
            }
            catch (UserCancelException /* err */) { /* user cancel */ }
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
            var query = !string.IsNullOrEmpty(Runtime?.Password) || Request.Password ?
                        new Query<string, string>(x => RaisePasswordRequired(x)) :
                        null;

            this.LogDebug(string.Format("Format:{0}\tMethod:{1}",
                fmt, Runtime?.CompressionMethod ?? CompressionMethod.Default));

            using (var writer = new ArchiveWriter(fmt, IO))
            {
                writer.Option = Runtime?.ToOption();
                foreach (var item in Request.Sources) writer.Add(item);
                ProgressStart();
                writer.Save(dest, query, CreateInnerProgress(x => ProgressReport = x));
            }

            // Move
            if (string.IsNullOrEmpty(Tmp) || !IO.Get(Tmp).Exists) return;
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
                    return Request.Format;
                case Format.Sfx:
                    Runtime = new ArchiveRuntimeSettings(f);
                    return Runtime.Format;
                case Format.BZip2:
                case Format.GZip:
                case Format.XZ:
                    Runtime = new ArchiveRuntimeSettings(Format.Tar);
                    Runtime.CompressionMethod = f.ToMethod();
                    return Runtime.Format;
                default:
                    RaiseRuntimeSettingsRequired();
                    return Runtime.Format;
            }
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
            if (!string.IsNullOrEmpty(Runtime?.Path)) Destination = Runtime.Path;
            else
            {
                SetDestination(Settings.Value.Archive, Request.Format.ToString());
                var name = IO.Get(Request.Sources.First()).NameWithoutExtension;
                Destination = IO.Combine(Destination, $"{name}{format.ToExtension()}");
            }

            var info = IO.Get(Destination);
            if (!info.Exists) return Destination;

            SetTmp(info.DirectoryName);
            return Tmp;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RaiseRuntimeSettingsRequired
        /// 
        /// <summary>
        /// RuntimeSettingsRequired イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void RaiseRuntimeSettingsRequired()
        {
            var info = IO.Get(Request.Sources.First());
            var path = IO.Combine(info.DirectoryName, $"{info.NameWithoutExtension}.zip");
            var args = new QueryEventArgs<string, ArchiveRuntimeSettings>(path);
            OnRuntimeSettingsRequired(args);
            Runtime = args.Result;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// RaisePasswordRequired
        /// 
        /// <summary>
        /// 必要に応じて PasswordRequired イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void RaisePasswordRequired(QueryEventArgs<string, string> e)
        {
            if (!string.IsNullOrEmpty(Runtime?.Password))
            {
                e.Result = Runtime.Password;
                e.Cancel = false;
            }
            else OnPasswordRequired(e);
        }

        #endregion
    }
}