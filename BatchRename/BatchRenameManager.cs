﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatchRename.UtilsClass;

namespace BatchRename
{
    class BatchRenameError
    {
        public int NameErrorIndex { get; set; }
        public string LastNameValue { get; set; }
        public string Message { get; set; }
    }

   
    class BatchRenameManager
    {
        private List<FileInfo> FileList;
        private List<string> NewFileNames;

        private List<DirectoryInfo> FolderList;
        private List<string> NewFolderNames;

        private List<BatchRenameError> errors;
        private int DuplicateMode;

        /// <summary>
        /// create manager to manage String Batch Renaming
        /// </summary>
        /// <param name="StringNames">names wanted to change</param>
        /// <param name="Operations">String operation wanted to perform on input names</param>
        public BatchRenameManager()
        {

            errors = new List<BatchRenameError>();
            FileList = new List<FileInfo>();
            NewFileNames = new List<string>();
        }

        public List<string> GetErrorList()
        {
            List<string> result = new List<string>();
            for (int i = 0; i < FileList.Count; i++) //fill list with default vaule
            {
                result.Add("None");
            }
            for (int i = 0; i < errors.Count; i++)
            {
                int ErrorIndex = errors[i].NameErrorIndex;
                string Message = errors[i].Message;
                result[ErrorIndex] = Message;
            }
            return result;
        }

        private bool isInErrorList(int index)
        {
            for (int i = 0; i < errors.Count; i++)
            {
                if (index == errors[i].NameErrorIndex)
                    return true;
            }
            return false;
        }

        public List<FileObj> BatchRename(List<FileObj> fileList, List<StringOperation> operations)
        { 
            List<FileObj> result = new List<FileObj>(fileList);
            if (NewFileNames.Count != 0) // clear list to save new changed names
            {
                NewFileNames.Clear();
            }

            for (int i = 0; i < fileList.Count; i++)
            {
                string path = fileList[i].Path + "\\" + fileList[i].Name;
                FileInfo fileInfo = new FileInfo(path);
                FileList.Add(fileInfo);
                NewFileNames.Add(Path.GetFileNameWithoutExtension(fileList[i].Name));
            }

            

            for (int i = 0; i < operations.Count; i++)
            {

                for (int j = 0; j < NewFileNames.Count; j++)
                {
                    /*If the name is in error list, skip the rename process, to preserve the pre-error value*/
                    bool IsInErrorList = isInErrorList(j);
                    if (IsInErrorList)
                        continue;
                    try
                    {
                        NewFileNames[j] = operations[i].OperateString(NewFileNames[j]); // perform operation
                    }
                    catch (Exception e) //if operation has failed
                    {
                        BatchRenameError error = new BatchRenameError()
                        {
                            NameErrorIndex = j, // save the position of the string which caused the error
                            LastNameValue = NewFileNames[j], //save the last values of the string before error
                            Message = e.Message, //the error message
                        };
                        errors.Add(error);
                    }
                }
            }
           
            //attach file name with its file extension and error messages that goes along with it if there's one
            List<string> ErrorMessages = GetErrorList();

            for (int i = 0; i <fileList.Count; i++)
            {
                NewFileNames[i] += FileList[i].Extension;
                result[i].NewName = NewFileNames[i];
                result[i].Error = ErrorMessages[i];
           
            }

            HandleDuplicateFiles();
            for (int i = 0; i < NewFileNames.Count; i++)
            {
                if (result[i].NewName != NewFileNames[i])
                {
                    result[i].NewName = NewFileNames[i];
                    result[i].Error = "Name changed to avoid duplication";
                }
            }
            return result;

        }

        public List<FolderObj> BatchRename(List<FolderObj> folderList, List<StringOperation> operations)
        {
            List<FolderObj> result = new List<FolderObj>(folderList);
            if (NewFolderNames.Count != 0) // clear list to save new changed names
            {
                NewFolderNames.Clear();
            }

            for (int i = 0; i < folderList.Count; i++)
            {
                string path = folderList[i].Path + "\\" + folderList[i].Name;
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                FolderList.Add(directoryInfo);
                NewFolderNames.Add(directoryInfo.Name);
                Debug.WriteLine(directoryInfo.Name);
            }



            for (int i = 0; i < operations.Count; i++)
            {

                for (int j = 0; j < NewFolderNames.Count; j++)
                {
                    /*If the name is in error list, skip the rename process, to preserve the pre-error value*/
                    bool IsInErrorList = isInErrorList(j);
                    if (IsInErrorList)
                        continue;
                    try
                    {
                        NewFolderNames[j] = operations[i].OperateString(NewFolderNames[j]); // perform operation
                    }
                    catch (Exception e) //if operation has failed
                    {
                        BatchRenameError error = new BatchRenameError()
                        {
                            NameErrorIndex = j, // save the position of the string which caused the error
                            LastNameValue = NewFolderNames[j], //save the last values of the string before error
                            Message = e.Message, //the error message
                        };
                        errors.Add(error);
                    }
                }
            }

            //send back error messages that goes along with the folder name if there's one
            List<string> ErrorMessages = GetErrorList();

            for (int i = 0; i < folderList.Count; i++)
            {
                result[i].NewName = NewFolderNames[i];
                result[i].Error = ErrorMessages[i];

            }


            return result;

        }

        private void nameDuplicateKeys(string duplicate)
        {
            int count = 0;

            /*The first value will not be numbered*/
            if (DuplicateMode == 0)
            {
                for (int i = 0; i < NewFileNames.Count; i++)
                {
                    if (NewFileNames[i] == duplicate)
                    {
                        if (count == 0)
                        {
                            count++;
                        }
                        else
                        {
                            string newName = NewFileNames[i] + count.ToString();
                            
                            count++;
                        }
                    }
                }
            }

            /*The first vaule will be numbered 1*/
            if (DuplicateMode == 1)
            {
                
                for (int i = 0; i < NewFileNames.Count; i++)
                {
                    if (NewFileNames[i] == duplicate)
                    {
                        count++;
                        NewFileNames[i] += count.ToString();
                    }
                }
            }

            /* duplicated value except the first one will be revert back to old value*/
            if (DuplicateMode == -1)
            {
                for (int i = 0; i < NewFileNames.Count; i++)
                {
                    if (NewFileNames[i] == duplicate)
                    {
                        if (count == 0)
                        {
                            count++;
                        }
                        else
                        {
                            NewFileNames[i] = FileList[i].Name;
                            
                        }
                    }
                }
            }
        }


        private void CheckDuplicate(int index)
        {
            for (int i = 0; i < NewFileNames.Count; i++)
            {

            }
        }

        private void HandleDuplicateFiles()
        {
            //List<List<int>> DuplicatePositions = new List<List<int>>();
            //List<String> DuplicateVaules = new List<string>();

            //var duplicateKeys = NewFileNames.GroupBy(x => x).Where(group => group.Count() > 1).Select(group => group.Key).ToList();
            //for (int i = 0; i <NewFileNames.Count; i++)
            //{

            //}

            var duplicateKeys = NewFileNames.GroupBy(x => x).Where(group => group.Count() > 1).Select(group => group.Key).ToList();
            if (duplicateKeys.Count == 0)
            {
                Debug.WriteLine("No values");
                return;
            }
           

            for (int i = 0; i < NewFileNames.Count; i++)
            {
                int count = 0;
                bool isDuplicate = true;
                string newName = NewFileNames[i];

                //Change duplicated value till it's not the case
                while (isDuplicate)
                {
                    isDuplicate = false;

                    //check upper part of the list
                    for (int j = 0; j < i; j++)
                    {
                        if (newName == NewFileNames[j])
                        {
                            isDuplicate = true;
                            count++;
                            newName = Path.GetFileNameWithoutExtension(NewFileNames[i]) + "_" + count.ToString() + FileList[i].Extension;
                        }
                    }

                    //check lower part of the list
                    for (int j = i + 1; j >NewFileNames.Count ; j++)
                    {
                        if (newName == NewFileNames[j])
                        {
                            isDuplicate = true;
                            count++;
                            NewFileNames[j] = NewFileNames[i] + "_" + count.ToString() + FileList[j].Extension;

                        }
                    }
                }
                NewFileNames[i] = newName;
            }
        }


        public void CommitChange()
        {
            
            for (int i = 0; i < FileList.Count; i++)
            {
                string newPath = FileList[i].Directory + "\\" + NewFileNames[i];
            }


        }

        //public List<string> StartBatching(string[] names, List<StringOperation> operations)
        //{
        //    NameCount = names.Length;
        //    List<string> result = new List<string>(names);

        //    for (int i = 0; i < operations.Count; i++)
        //    {

        //        for (int j = 0; j < result.Count; j++)
        //        {
        //            try
        //            {
        //                result[j] = operations[i].OperateString(result[j]); // perform operation
        //            }
        //            catch (Exception e) //if operation has failed
        //            {
        //                BatchRenameError error = new BatchRenameError()
        //                {
        //                    NameErrorIndex = j, // save the position of the string which caused the error
        //                    LastNameValue = names[j], //save the last values of the string before error
        //                    Message = e.Message, //the error message
        //                };
        //                errors.Add(error);
        //                result.RemoveAt(j); //remove the string from the Batch reanaming list so that it can't be changed by later Operations
        //                j--;
        //            }
        //        }
        //    }
        //    //add the faulty names back to the list
        //    for (int i = 0; i < errors.Count; i++)
        //    {
        //        string ErrorString = errors[i].LastNameValue;
        //        int index = errors[i].NameErrorIndex;
        //        result.Insert(index, ErrorString);
        //    }
        //    return result;
        //}

        
    }


}
