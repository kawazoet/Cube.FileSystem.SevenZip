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
using System.Collections.Generic;
using Cube.Enumerations;

namespace Cube.FileSystem.SevenZip.Ice
{
    /* --------------------------------------------------------------------- */
    ///
    /// PresetMenuConversions
    ///
    /// <summary>
    /// PresetMenu に関する拡張メソッドを定義するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public static class PresetMenuConversions
    {
        /* --------------------------------------------------------------------- */
        ///
        /// ToArguments
        ///
        /// <summary>
        /// PresetMenu に対応するプログラム引数を取得します。
        /// </summary>
        /// 
        /// <param name="menu">メニュー</param>
        /// 
        /// <returns>プログラム引数</returns>
        /// 
        /// <remarks>
        /// 変換可能な PresetMenu が複数存在する場合、最初に見つかった
        /// メニューに対応する引数が返されます。
        /// </remarks>
        ///
        /* --------------------------------------------------------------------- */
        public static IEnumerable<string> ToArguments(this PresetMenu menu)
        {
            if ((menu & PresetMenu.ArchiveOptions) != 0) return ToArchive(menu);
            if ((menu & PresetMenu.ExtractOptions) != 0) return ToExtract(menu);
            if ((menu & PresetMenu.MailOptions) != 0) return ToMail(menu);
            if ((menu & PresetMenu.Archive) != 0) return ToArchive(PresetMenu.ArchiveZip);
            if ((menu & PresetMenu.Extract) != 0) return new[] { "/x" };
            if ((menu & PresetMenu.Mail) != 0) return ToMail(PresetMenu.MailZip);
            return new string[0];
        }

        #region Implementations

        /* --------------------------------------------------------------------- */
        ///
        /// ToArchive
        ///
        /// <summary>
        /// 圧縮に関する PresetMenu に対応するプログラム引数を取得します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        private static IEnumerable<string> ToArchive(PresetMenu m)
        {
            if (m.HasFlag(PresetMenu.ArchiveZip)) return new[] { "/c:zip" };
            if (m.HasFlag(PresetMenu.ArchiveZipPassword)) return new[] { "/c:zip", "/p" };
            if (m.HasFlag(PresetMenu.ArchiveSevenZip)) return new[] { "/c:7z" };
            if (m.HasFlag(PresetMenu.ArchiveBZip2)) return new[] { "/c:bzip2" };
            if (m.HasFlag(PresetMenu.ArchiveGZip)) return new[] { "/c:gzip" };
            if (m.HasFlag(PresetMenu.ArchiveXZ)) return new[] { "/c:xz" };
            if (m.HasFlag(PresetMenu.ArchiveSfx)) return new[] { "/c:exe" };
            if (m.HasFlag(PresetMenu.ArchiveDetail)) return new[] { "/c:detail" };
            return new string[0];
        }

        /* --------------------------------------------------------------------- */
        ///
        /// ToExtract
        ///
        /// <summary>
        /// 解凍に関する PresetMenu に対応するプログラム引数を取得します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        private static IEnumerable<string> ToExtract(PresetMenu m)
        {
            if (m.HasFlag(PresetMenu.ExtractDesktop)) return new[] { "/x", "/out:desktop" };
            if (m.HasFlag(PresetMenu.ExtractMyDocuments)) return new[] { "/x", "/out:mydocuments" };
            if (m.HasFlag(PresetMenu.ExtractRuntime)) return new[] { "/x", "/out:runtime" };
            if (m.HasFlag(PresetMenu.ExtractSource)) return new[] { "/x", "/out:source" };
            return new string[0];
        }

        /* --------------------------------------------------------------------- */
        ///
        /// ToMail
        ///
        /// <summary>
        /// 圧縮してメール送信に関する PresetMenu に対応するプログラム引数を
        /// 取得します。
        /// </summary>
        ///
        /* --------------------------------------------------------------------- */
        private static IEnumerable<string> ToMail(PresetMenu m)
        {
            if (m.HasFlag(PresetMenu.MailZip)) return new[] { "/c:zip", "/m" };
            if (m.HasFlag(PresetMenu.MailZipPassword)) return new[] { "/c:zip", "/p", "/m" };
            if (m.HasFlag(PresetMenu.MailSevenZip)) return new[] { "/c:7z", "/m" };
            if (m.HasFlag(PresetMenu.MailBZip2)) return new[] { "/c:bzip2", "/m" };
            if (m.HasFlag(PresetMenu.MailGZip)) return new[] { "/c:gzip", "/m" };
            if (m.HasFlag(PresetMenu.MailXZ)) return new[] { "/c:xz", "/m" };
            if (m.HasFlag(PresetMenu.MailSfx)) return new[] { "/c:exe", "/m" };
            if (m.HasFlag(PresetMenu.MailDetail)) return new[] { "/c:detail", "/m" };
            return new string[0];
        }

        #endregion
    }
}
