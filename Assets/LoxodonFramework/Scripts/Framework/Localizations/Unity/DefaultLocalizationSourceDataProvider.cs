﻿/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
 * of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 */

using Loxodon.Log;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Loxodon.Framework.Localizations
{
    /// <summary>
    /// Resources data provider.
    /// dir:
    /// root/default/
    /// 
    /// root/zh/
    /// root/zh-CN/
    /// root/zh-TW/
    /// root/zh-HK/
    /// 
    /// root/en/
    /// root/en-US/
    /// root/en-CA/
    /// root/en-AU/
    /// </summary>
    public class DefaultLocalizationSourceDataProvider : IDataProvider
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DefaultLocalizationSourceDataProvider));

        protected string[] filenames;
        protected string root;

        public DefaultLocalizationSourceDataProvider(string root, params string[] filenames)
        {
            this.root = root;
            this.filenames = filenames;
        }


        protected string GetDefaultPath(string filename)
        {
            return GetPath("default", filename);
        }

        protected string GetPath(string dir, string filename)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(this.root);
            if (!this.root.EndsWith("/"))
                buf.Append("/");
            buf.Append(dir).Append("/").Append(filename.Replace(".asset", ""));
            return buf.ToString();
        }

        public virtual void Load(CultureInfo cultureInfo, Action<Dictionary<string, object>> onLoadCompleted)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            try
            {
                foreach (string filename in filenames)
                {
                    try
                    {
                        LocalizationSourceAsset defaultSourceAsset = Resources.Load<LocalizationSourceAsset>(GetDefaultPath(filename)); //eg:default
                        LocalizationSourceAsset twoLetterISOSourceAsset = Resources.Load<LocalizationSourceAsset>(GetPath(cultureInfo.TwoLetterISOLanguageName, filename));//eg:zh  en
                        LocalizationSourceAsset sourceAsset = cultureInfo.Name.Equals(cultureInfo.TwoLetterISOLanguageName) ? null : Resources.Load<LocalizationSourceAsset>(GetPath(cultureInfo.Name, filename));//eg:zh-CN  en-US

                        if (defaultSourceAsset != null)
                            FillData(dict, defaultSourceAsset.Source);
                        if (twoLetterISOSourceAsset != null)
                            FillData(dict, twoLetterISOSourceAsset.Source);
                        if (sourceAsset != null)
                            FillData(dict, sourceAsset.Source);
                    }
                    catch (Exception e)
                    {
                        if (log.IsWarnEnabled)
                            log.WarnFormat("An error occurred when loading localized data from \"{0}\".Error:{1}", filename, e);
                    }
                }
            }
            finally
            {
                if (onLoadCompleted != null)
                    onLoadCompleted(dict);
            }
        }

        private void FillData(Dictionary<string, object> dict, MonolingualSource source)
        {
            if (source == null)
                return;

            foreach (KeyValuePair<string, object> kv in source.GetData())
            {
                dict[kv.Key] = kv.Value;
            }
        }
    }
}
