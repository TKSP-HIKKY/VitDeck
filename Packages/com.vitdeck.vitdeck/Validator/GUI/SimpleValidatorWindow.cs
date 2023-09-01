﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text;
using VitDeck.Language;
using VitDeck.Utilities;

namespace VitDeck.Validator.GUI
{
    public class SimpleValidatorWindow : EditorWindow
    {
        private IRuleSet ruleSet;
        private DefaultAsset baseFolder;

        private bool isHideInfoMessage;
        private bool isOpenResultLogArea;
        private bool isOpenMessageArea = true;
        private Vector2 resultLogAreaScroll;
        private Vector2 messageAreaScroll;
        private string validationLog;

        private List<Message> messages;


        public static void Validate(IRuleSet ruleSet, DefaultAsset baseFolder)
        {
            var window = GetWindow<SimpleValidatorWindow>(false, "VitDeck");
            window.ruleSet = ruleSet;
            window.baseFolder = baseFolder;
            window.Show();
            window.Validate();
        }

        [MenuItem("VitDeck/Validate(Simple)")]
        static void Open()
        {
            var window = GetWindow<ValidatorWindow>(false, "VitDeck");
            window.Show();
        }


        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 80;
            EditorGUILayout.LabelField("Rule Checker");
            //Rule set dropdown
            EditorGUILayout.LabelField("Rule Set:", ruleSet.RuleSetName);
            //Base folder field
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Base Folder:", baseFolder, typeof(DefaultAsset), true);
            EditorGUI.EndDisabledGroup();

            //Log Option
            isHideInfoMessage = EditorGUILayout.ToggleLeft("Show only errors & warnings", isHideInfoMessage);
            //Check button
            if (GUILayout.Button("Check"))
            {
                Validate();
            }

            //Result log
            isOpenResultLogArea = EditorGUILayout.Foldout(isOpenResultLogArea, "Result log");
            if (isOpenResultLogArea)
            {
                resultLogAreaScroll = EditorGUILayout.BeginScrollView(resultLogAreaScroll);
                GUILayout.TextArea(validationLog, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }

            //Help message
            if (messages != null)
            {
                var errorCount = messages.Count(m => m.type == MessageType.Error);
                var warningCount = messages.Count(m => m.type == MessageType.Warning);
                var infoCount = messages.Count(m => m.type == MessageType.Info);
                var sum = errorCount + warningCount + infoCount;
                var countMessage = string.Format("{0}(Error:{1}  Warning:{2}  Informaiton:{3})", sum, errorCount,
                    warningCount, infoCount);
                isOpenMessageArea = EditorGUILayout.Foldout(isOpenMessageArea, "Messages:" + countMessage);
                if (isOpenMessageArea)
                {
                    messageAreaScroll = EditorGUILayout.BeginScrollView(messageAreaScroll);
                    foreach (var msg in messages)
                    {
                        if (msg.type < (isHideInfoMessage ? MessageType.Warning : MessageType.Info))
                            continue;
                        GetMessageBox(msg);
                    }

                    EditorGUILayout.EndScrollView();
                }
            }

            //Copy Button
            if (GUILayout.Button("Copy result log"))
            {
                EditorGUIUtility.systemCopyBuffer = validationLog;
            }
        }

        private void Validate()
        {
            if (ruleSet == null)
                return;
            ClearLogs();
            SaveSettings();
            var baseFolderPath = AssetDatabase.GetAssetPath(baseFolder);
            OutLog("Starting validation.");
            var results = Validator.Validate(ruleSet, baseFolderPath);
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"- version:{ProductInfoUtility.GetVersion()}");
            stringBuilder.AppendLine($"- Rule set:{ruleSet.RuleSetName}");
            stringBuilder.AppendLine($"- Base folder:{baseFolderPath}");
            AddMessages(stringBuilder.ToString(), results);
            stringBuilder.AppendLine(GetResultLog(results, isHideInfoMessage ? IssueLevel.Warning : IssueLevel.Info));
            OutLog(stringBuilder.ToString());
            OutLog("Validation complete.");
        }

        private void ClearLogs()
        {
            validationLog = "";
            messages = new List<Message>();
        }

        private void OutLog(string log)
        {
            Debug.Log(log);
            validationLog += log + System.Environment.NewLine;
        }

        private string GetResultLog(ValidationResult[] results, IssueLevel level)
        {
            var log = "";
            foreach (var result in results)
            {
                log += result.GetResultLog(true, level) + Environment.NewLine;
            }

            return log;
        }

        private void AddMessages(string header, ValidationResult[] results)
        {
            if (results.SelectMany(r => r.Issues).All(i => i.level != IssueLevel.Fatal))
            {
                messages.Add(new Message(
                    LocalizedMessage.Get("ValidatorWindow.ValidationCompleted") + Environment.NewLine + header,
                    MessageType.Info));
            }
            else
            {
                messages.Add(new Message(
                    LocalizedMessage.Get("ValidatorWindow.ValidationAborted") + Environment.NewLine + header,
                    MessageType.Error));
            }

            foreach (var result in results)
            {
                var ruleResultLog = result.GetResultLog(false);
                if (!string.IsNullOrEmpty(ruleResultLog))
                    messages.Add(new Message(ruleResultLog, MessageType.Info, null));
                foreach (var issue in result.Issues)
                {
                    var txt = result.RuleName + Environment.NewLine;
                    if (issue.target != null)
                        txt += issue.target.name + Environment.NewLine;
                    txt += issue.message + Environment.NewLine + issue.solution;
                    messages.Add(new Message(txt, GetMessageType(issue.level), issue));
                }
            }
        }

        private void SaveSettings()
        {
            var userSettings = SettingUtility.GetSettings<UserSettings>();
            userSettings.validatorFolderPath = AssetDatabase.GetAssetPath(baseFolder);
            if (ruleSet != null)
            {
                userSettings.validatorRuleSetType = ruleSet.GetType().Name;
            }

            SettingUtility.SaveSettings(userSettings);
        }

        private MessageType GetMessageType(IssueLevel level)
        {
            switch (level)
            {
                case IssueLevel.Info: return MessageType.Info;
                case IssueLevel.Warning: return MessageType.Warning;
                case IssueLevel.Error: return MessageType.Error;
                case IssueLevel.Fatal: return MessageType.Error;
                default: return MessageType.None;
            }
        }

        private void GetMessageBox(Message msg)
        {
            GUILayout.BeginHorizontal();

            var helpBoxRect = EditorGUILayout.BeginHorizontal();
            if (Event.current.type == EventType.MouseUp && helpBoxRect.Contains(Event.current.mousePosition) &&
                msg.issue != null)
                EditorGUIUtility.PingObject(msg.issue.target);
            EditorGUILayout.HelpBox(msg.message, msg.type, true);
            EditorGUILayout.EndHorizontal();
            if (msg.issue != null && !string.IsNullOrEmpty(msg.issue.solutionURL))
            {
                CustomGUILayout.URLButton("Help", msg.issue.solutionURL, GUILayout.Width(50));
            }
            else
            {
                GUILayout.Space(55);
            }

            GUILayout.EndHorizontal();
        }

        private class Message
        {
            public Message(string message, MessageType type, Issue issue = null)
            {
                this.message = message;
                this.type = type;
                this.issue = issue;
            }

            public Issue issue;
            public string message;
            public MessageType type;
        }
    }
}