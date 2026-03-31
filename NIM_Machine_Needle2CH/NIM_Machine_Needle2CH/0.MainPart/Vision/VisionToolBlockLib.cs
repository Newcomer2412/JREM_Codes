using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.Exceptions;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.Implementation;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.PMRedLine;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MachineControlBase
{
    /// <summary>
    /// 코그넥스 Tool Block 클래스
    /// </summary>
    public class CogToolBlockClass
    {
        /// <summary>
        /// Camera 번호
        /// </summary>
        public uint uiCameraNo = 0;

        /// <summary>
        /// Tool Block 번호
        /// </summary>
        public uint uiToolBlockNo = 0;

        /// <summary>
        /// 코그넥스 초기화 완료 플래그
        /// </summary>
        public bool bCogInitialize = false;

        /// <summary>
        /// CogToolBlock 클래스
        /// </summary>
        public CogToolBlockEditV2 cCogToolBlockEditV2 = new CogToolBlockEditV2();

        /// <summary>
        /// Vision UI 클래스
        /// </summary>
        public VisionToolBlockUI cVisionToolBlockUI = null;

        /// <summary>
        /// 코그넥스 이미지 파일 관리 툴
        /// </summary>
        public CogImageFileTool cImageFileTool = new CogImageFileTool();

        /// <summary>
        /// Vision 이미지 IDB 저장
        /// </summary>
        /// <param name="strInfo"></param>
        public void IDBFile_Save(string strInfo)
        {
            ImageData imageData = new ImageData();
            imageData.iImageType = eImageType.IDB;
            imageData.strPath = CMainLib.Ins.cVisionData.strVisionImagePath[uiCameraNo] +
                                string.Format(@"_{0}\{1}\", uiToolBlockNo, DateTime.Today.ToString("yyyyMMdd"));
            imageData.strFileName = imageData.strPath + DateTime.Now.ToString("HH_mm_ss_fff_") + strInfo + ".idb";
            if (cImageFileTool.InputImage != null)
            {
                imageData.ImageFile = cImageFileTool.InputImage.CopyBase(CogImageCopyModeConstants.CopyPixels);
            }
            CMainLib.Ins.cimageSaveProcess.SetSaveImage(imageData);
        }

        /// <summary>
        /// BMP File로 이미지 저장
        /// </summary>
        /// <param name="strInfo"></param>
        public void BMPFile_Save(string strInfo)
        {
            ImageData imageData = new ImageData();
            imageData.iImageType = eImageType.BMP;
            imageData.strPath = CMainLib.Ins.cVisionData.strVisionImagePath[uiCameraNo] +
                                string.Format(@"_{0}\{1}\", uiToolBlockNo, DateTime.Today.ToString("yyyyMMdd"));
            imageData.strFileName = imageData.strPath + DateTime.Now.ToString("HH_mm_ss_fff_") + strInfo + ".bmp";
            if (cImageFileTool.InputImage != null)
            {
                imageData.ImageFile = cImageFileTool.InputImage.CopyBase(CogImageCopyModeConstants.CopyPixels);
            }
            CMainLib.Ins.cimageSaveProcess.SetSaveImage(imageData);
        }

        /// <summary>
        /// File image를 ToolBlock에 전달
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ImageFileTool_Ran(object sender, EventArgs e)
        {
            cVisionToolBlockUI.GetCogDisplay().InteractiveGraphics.Clear();
            cVisionToolBlockUI.GetCogDisplay().Image = cImageFileTool.OutputImage;
            cVisionToolBlockUI.GetCogDisplay().AutoFit = true;
            cCogToolBlockEditV2.Subject.Inputs["InputImage"].Value = cImageFileTool.OutputImage;
        }

        /// <summary>
        /// 픽셀 값을 mm 값으로 변환
        /// </summary>
        /// <param name="cogTransform2DLinear"></param>
        /// <returns></returns>
        private CVisionResult PixelToMilimeter(CogTransform2DLinear cogTransform2DLinear)
        {
            CVisionResult cResult = new CVisionResult();
            if (cogTransform2DLinear != null)
            {
                cResult.dX = Math.Round((cogTransform2DLinear.TranslationX - CMainLib.Ins.cVisionData.iScreenHalfWidth[uiCameraNo]) *
                         CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);
                cResult.dY = Math.Round((cogTransform2DLinear.TranslationY - CMainLib.Ins.cVisionData.iScreenHalfHeight[uiCameraNo]) *
                                         CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);
                cResult.dT = Math.Round(cogTransform2DLinear.Rotation * (180 / Math.PI), 3);
            }
            return cResult;
        }

        /// <summary>
        /// 픽셀 값을 mm 값으로 변환
        /// </summary>
        /// <param name="cogCircle"></param>
        /// <returns></returns>
        private CVisionResult PixelToMilimeter(CogCircle cogCircle)
        {
            CVisionResult cResult = new CVisionResult();
            if (cogCircle != null)
            {
                cResult.dX = Math.Round((cogCircle.CenterX - CMainLib.Ins.cVisionData.iScreenHalfWidth[uiCameraNo]) *
                             CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);
                cResult.dY = Math.Round((cogCircle.CenterY - CMainLib.Ins.cVisionData.iScreenHalfHeight[uiCameraNo]) *
                                         CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);
                cResult.dT = Math.Round(cogCircle.Radius * (180 / Math.PI), 3);
            }
            return cResult;
        }

        /// <summary>
        /// Offset 값을 더함
        /// </summary>
        /// <param name="cVisionResult"></param>
        /// <returns></returns>
        private CVisionResult AddCamOffset(CVisionResult cVisionResult)
        {
            CCamOffset cCamOffset = CMainLib.Ins.cOptionData.GetVisionOffset(uiCameraNo, uiToolBlockNo);
            cVisionResult.dX = Math.Round(cVisionResult.dX + cCamOffset.dXOffset, 3);
            cVisionResult.dY = Math.Round(cVisionResult.dY + cCamOffset.dYOffset, 3);
            cVisionResult.dT = Math.Round(cVisionResult.dT + cCamOffset.dTOffset, 3);
            return cVisionResult;
        }

        /// <summary>
        /// 화면에 값 표현
        /// </summary>
        /// <param name="strResult"></param>
        /// <param name="iX"></param>
        /// <param name="iY"></param>
        private void ResultAddInteractiveGraphics(string strResult, int iX, int iY)
        {
            CogGraphicLabel cogValue = new CogGraphicLabel();
            cogValue.Text = strResult;
            cogValue.X = iX;
            cogValue.Y = iY;
            cogValue.Color = CogColorConstants.Green;
            cogValue.Font = new Font("Area", 12, FontStyle.Regular);
            cVisionToolBlockUI.AddInteractiveGraphics(cogValue, null, false);
        }

        /// <summary>
        /// 원하는 위치에 값 표현
        /// </summary>
        /// <param name="iDisplayIndex"></param>
        /// <param name="strResult"></param>
        /// <param name="iX"></param>
        /// <param name="iY"></param>
        private void ResultAddInteractiveGraphics(int iDisplayIndex, string strResult, int iX, int iY)
        {
            CogGraphicLabel cogValue = new CogGraphicLabel();
            cogValue.Text = strResult;
            cogValue.X = iX;
            cogValue.Y = iY;
            cogValue.Color = CogColorConstants.Red;
            cogValue.Font = new Font("Area", 12, FontStyle.Regular);
            cVisionToolBlockUI.AddInteractiveGraphics(iDisplayIndex, cogValue, null, false);
        }

        /// <summary>
        /// 화면에 Good Ng 값 표현
        /// </summary>
        /// <param name="strGoodNg"></param>
        /// <param name="iX"></param>
        /// <param name="iY"></param>
        private void GoodNgAddInteractiveGraphics(string strGoodNg, int iX, int iY)
        {
            CogGraphicLabel cogValue = new CogGraphicLabel();
            cogValue.X = iX;
            cogValue.Y = iY;
            cogValue.Text = strGoodNg;
            if (strGoodNg == "OK") cogValue.Color = CogColorConstants.Blue;
            else cogValue.Color = CogColorConstants.Red;
            cogValue.Font = new Font("Area", 20, FontStyle.Bold);
            cVisionToolBlockUI.AddInteractiveGraphics(cogValue, null, false);
        }

        /// <summary>
        /// ToolBlock Ran 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ToolBlock_Ran(object sender, EventArgs e)
        {
            if (bCogInitialize == false) return;
            // Vision 이미지를 Backup 해야할 경우 저장
            Parallel.Invoke(
                            () =>
                            {
                                if (uiCameraNo == (int)eCAM.CAM0_PipeNeedlePickUp) // 파이프, 니들 픽업 서칭
                                {
                                    CAM0_FindPickPipeNeedle();
                                }
                                else if (uiCameraNo == (int)eCAM.CAM1_Pipe) // 파이프 확인
                                {
                                    CAM1_CheckPipePosture();
                                }
                                else if (uiCameraNo == (int)eCAM.CAM2_Needle) // 니들 방향 확인
                                {
                                    CAM2_CheckNeedlePosture();
                                }
                                else if (uiCameraNo == (int)eCAM.CAM3_PipeMount) // 파이프 마운트
                                {
                                    CAM3_PipeMount();
                                }
                                else if (uiCameraNo == (int)eCAM.CAM4_NeedleMount) // 니들 마운트
                                {
                                    CAM4_NeedleMount();
                                }
                                else if (uiCameraNo == (int)eCAM.CAM5_Dispenser) // 홀더 디스펜서
                                {
                                    CAM5_DispenserAlign();
                                }
                                else   // 기타 CAM 검사
                                {
                                    Else_CAM();
                                }
                            },
                            () =>
                            {
                                // Vision 이미지를 Backup 해야할 경우 저장
                                if (CMainLib.Ins.cOptionData.bImage_SaveUse == true &&
                                    CMainLib.Ins.cOptionData.bImage_NGSaveUse == false)
                                {
                                    BMPFile_Save("FullImage");
                                }
                            });
        }

        /// <summary>
        /// 픽업할 파이프, 니들 찾기
        /// </summary>
        private void CAM0_FindPickPipeNeedle()
        {
            try
            {
                // 툴블록 뷰의 화면 선택 콤보 박스에서 0번째 화면의 뷰를 가져와서 Vision 화면에 적용한다.
                if (cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0] != null)
                    cVisionToolBlockUI.CogRecord((CogRecord)cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0]);

                CVisionResultData cVisionResultData = CMainLib.Ins.cVar.GetVisionResultData(uiCameraNo, uiToolBlockNo);
                CogPMAlignTool[] PipeNeeldePMAlign = new CogPMAlignTool[2];
                PipeNeeldePMAlign[0] = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool1"] as CogPMAlignTool;
                PipeNeeldePMAlign[1] = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool2"] as CogPMAlignTool;

                if (PipeNeeldePMAlign[0] != null && PipeNeeldePMAlign[0].Results != null)
                {
                    if (PipeNeeldePMAlign[0].Results.Count >= 1)
                    {
                        cVisionResultData.uiPipeCount = (uint)PipeNeeldePMAlign[0].Results.Count;
                        cVisionResultData.dPipeX = Math.Round((PipeNeeldePMAlign[0].Results[0].GetPose().TranslationX - CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM0_PipePnP_X_CameraCal) * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);
                        cVisionResultData.dPipeY = Math.Round((CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM0_PipePnP_Y_CameraCal - PipeNeeldePMAlign[0].Results[0].GetPose().TranslationY) * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);
                        cVisionResultData.dPipeDegree = Math.Round(PipeNeeldePMAlign[0].Results[0].GetPose().Rotation / Math.PI * 180, 3);
                        NLogger.AddLog(eLogType.CAM0_PipeNeedlePickUp + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Pick Pipe : OK");
                    }
                }

                if (PipeNeeldePMAlign[1] != null && PipeNeeldePMAlign[1].Results != null)
                {
                    if (PipeNeeldePMAlign[1].Results.Count >= 1)
                    {
                        cVisionResultData.uiNeedleCount = (uint)PipeNeeldePMAlign[1].Results.Count;
                        cVisionResultData.dNeedleX = Math.Round((PipeNeeldePMAlign[1].Results[0].GetPose().TranslationX - CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM0_NeedlePnP_X_CameraCal) * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);
                        cVisionResultData.dNeedleY = Math.Round((CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM0_NeedlePnP_Y_CameraCal - PipeNeeldePMAlign[1].Results[0].GetPose().TranslationY) * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);
                        cVisionResultData.dNeedleDegree = Math.Round(PipeNeeldePMAlign[1].Results[0].GetPose().Rotation / Math.PI * 180, 3);
                        NLogger.AddLog(eLogType.CAM0_PipeNeedlePickUp + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Pick Needle : OK");
                    }
                }

                Parallel.Invoke(() =>
                {
                    // 메인 화면에 표시하는 CogDisplay Index 변경, DB 저장
                    if (CMainLib.Ins.McState == eMachineState.RUN)
                    {
                        CMainLib.Ins.SetDBData(uiCameraNo, uiToolBlockNo, true);
                        cVisionToolBlockUI.CogDisplayNextIndex();
                    }
                });
                //() =>
                //{
                //    // NG Vision 이미지를 Backup 해야할 경우 저장
                //    if (cVisionResultData.bGoodNg == false &&
                //        CMainLib.Ins.cOptionData.bImage_SaveUse == true &&
                //        CMainLib.Ins.cOptionData.bImage_NGSaveUse == true)
                //    {
                //        BMPFile_Save("FullImage");
                //    }
                //});

                cVisionResultData.bShootFinish = true;
            }
            catch (Exception ex)
            {
                NLogger.AddLog(eLogType.CAM0_PipeNeedlePickUp + (int)uiCameraNo, NLogger.eLogLevel.ERROR, ex.ToString(), false);
            }
        }

        /// <summary>
        /// CAM1 파이프 자세 확인
        /// </summary>
        private void CAM1_CheckPipePosture()
        {
            try
            {
                // 툴블록 뷰의 화면 선택 콤보 박스에서 0번째 화면의 뷰를 가져와서 Vision 화면에 적용한다.
                if (cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0] != null)
                    cVisionToolBlockUI.CogRecord((CogRecord)cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0]);

                CVisionResultData cVisionResultData = CMainLib.Ins.cVar.GetVisionResultData(uiCameraNo, uiToolBlockNo);
                CogPMAlignTool PipePMAlign = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool1"] as CogPMAlignTool;
                CogCaliperTool PipeCaliper = cCogToolBlockEditV2.Subject.Tools["CogCaliperTool1"] as CogCaliperTool;
                CogFindLineTool CogFindLine1 = cCogToolBlockEditV2.Subject.Tools["CogFindLineTool1"] as CogFindLineTool;
                CogFindLineTool CogFindLine2 = cCogToolBlockEditV2.Subject.Tools["CogFindLineTool2"] as CogFindLineTool;
                bool bNG = false;

                if (PipePMAlign.Results != null && PipeCaliper.Results != null &&
                    CogFindLine1.Results != null && CogFindLine2.Results != null)
                {
                    if (PipeCaliper.Results.Count >= 2)
                    {
                        double PipeHeight = PipeCaliper.Results[0].PositionY - PipeCaliper.Results[1].PositionY;
                        if (Math.Abs(PipeHeight) > 180)
                        {
                            GoodNgAddInteractiveGraphics("Fail", 1200, 2600);
                            NLogger.AddLog(eLogType.CAM1_PipePosture + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Needle Amount : NG");
                            cVisionResultData.bPipeDoubleCatch = true;
                            cVisionResultData.bGoodNg = false;
                            bNG = true;
                        }
                        else
                        {
                            cVisionResultData.bPipeDoubleCatch = false;

                            if (PipePMAlign != null && PipePMAlign.Results != null)
                            {
                                if (PipePMAlign.Results.Count >= 1)
                                {
                                    NLogger.AddLog(eLogType.CAM1_PipePosture + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Needle PM Align 180");
                                    cVisionResultData.bGoodNg = false;
                                    bNG = true;
                                }
                                else
                                {
                                    if (CogFindLine1 != null && CogFindLine1.Results.GetLine() == null)
                                    {
                                        GoodNgAddInteractiveGraphics("Fail", 1200, 2600);
                                        NLogger.AddLog(eLogType.CAM1_PipePosture + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Needle Height : NG");
                                        cVisionResultData.bGoodNg = false;
                                        bNG = true;
                                    }

                                    if (CogFindLine2 != null && CogFindLine2.Results.GetLine() != null)
                                    {
                                        GoodNgAddInteractiveGraphics("Fail", 1200, 2600);
                                        NLogger.AddLog(eLogType.CAM1_PipePosture + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Needle Height : NG");
                                        cVisionResultData.bGoodNg = false;
                                        bNG = true;
                                    }

                                    if (bNG == false)
                                    {
                                        GoodNgAddInteractiveGraphics("OK", 1200, 2600);
                                        cVisionResultData.bGoodNg = true;
                                        NLogger.AddLog(eLogType.CAM1_PipePosture + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Needle Posture : OK");
                                    }
                                }
                            }
                        }
                    }

                    Parallel.Invoke(() =>
                    {
                        // 메인 화면에 표시하는 CogDisplay Index 변경, DB 저장
                        if (CMainLib.Ins.McState == eMachineState.RUN)
                        {
                            CMainLib.Ins.SetDBData(uiCameraNo, uiToolBlockNo, true);
                            cVisionToolBlockUI.CogDisplayNextIndex();
                        }
                    });
                    //() =>
                    //{
                    //    // NG Vision 이미지를 Backup 해야할 경우 저장
                    //    if (cVisionResultData.bGoodNg == false &&
                    //        CMainLib.Ins.cOptionData.bImage_SaveUse == true &&
                    //        CMainLib.Ins.cOptionData.bImage_NGSaveUse == true)
                    //    {
                    //        BMPFile_Save("FullImage");
                    //    }
                    //});

                    cVisionResultData.bShootFinish = true;
                }
                else
                {
                    StringBuilder strlog = null;
                    if (PipePMAlign.Results == null)
                    {
                        strlog.Append("PMAlign.Results, ");
                    }
                    if (PipeCaliper.Results == null)
                    {
                        strlog.Append("Caliper.Results, ");
                    }
                    if (CogFindLine1.Results == null)
                    {
                        strlog.Append("FindLine1.Results, ");
                    }
                    if (CogFindLine2.Results == null)
                    {
                        strlog.Append("FindLine2.Results, ");
                    }
                    strlog.Append(" IS NULL");
                    NLogger.AddLog(eLogType.CAM2_NeedlePosture + (int)uiCameraNo, NLogger.eLogLevel.ERROR, strlog.ToString(), false);

                    cVisionResultData.bShootFinish = false;
                }
            }
            catch (Exception ex)
            {
                NLogger.AddLog(eLogType.CAM2_NeedlePosture + (int)uiCameraNo, NLogger.eLogLevel.ERROR, ex.ToString(), false);
            }

            //try
            //{
            //    // 툴블록 뷰의 화면 선택 콤보 박스에서 0번째 화면의 뷰를 가져와서 Vision 화면에 적용한다.
            //    if (cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0] != null)
            //        cVisionToolBlockUI.CogRecord((CogRecord)cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0]);

            //    CVisionResultData cVisionResultData = CMainLib.Ins.cVar.GetVisionResultData(uiCameraNo, uiToolBlockNo);
            //    CogPMAlignTool PipePMAlign = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool1"] as CogPMAlignTool;

            //    if (PipePMAlign != null && PipePMAlign.Results != null &&
            //        PipePMAlign.Results.Count >= 1)
            //    {
            //        cVisionResultData.dDegree = Math.Round(PipePMAlign.Results[0].GetPose().Rotation * Math.PI / 180, 3);
            //        if (PipePMAlign.Results[0].GetPose().TranslationX >= 1700 &&
            //            Math.Abs(cVisionResultData.dDegree) <= CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM1_TransferPipeDegreeLimit)
            //        {
            //            cVisionResultData.dPipeClampZ = Math.Round((PipePMAlign.Results[0].GetPose().TranslationX - 2009) * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);
            //            GoodNgAddInteractiveGraphics("OK", 1200, 2600);
            //            NLogger.AddLog(eLogType.CAM1_PipePosture + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Pipe Posture : OK");
            //        }
            //        else
            //        {
            //            GoodNgAddInteractiveGraphics("Fail", 1200, 2600);
            //            NLogger.AddLog(eLogType.CAM1_PipePosture + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Pipe Posture : NG");
            //            cVisionResultData.bGoodNg = false;
            //        }
            //    }
            //    else
            //    {
            //        GoodNgAddInteractiveGraphics("Fail", 1200, 2600);
            //        NLogger.AddLog(eLogType.CAM1_PipePosture + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Pipe PM Align : NG");
            //        cVisionResultData.bGoodNg = false;
            //    }

            //    Parallel.Invoke(() =>
            //    {
            //        // 메인 화면에 표시하는 CogDisplay Index 변경, DB 저장
            //        if (CMainLib.Ins.McState == eMachineState.RUN)
            //        {
            //            CMainLib.Ins.SetDBData(uiCameraNo, uiToolBlockNo, true);
            //            cVisionToolBlockUI.CogDisplayNextIndex();
            //        }
            //    });
            //    //() =>
            //    //{
            //    //    // NG Vision 이미지를 Backup 해야할 경우 저장
            //    //    if (cVisionResultData.bGoodNg == false &&
            //    //        CMainLib.Ins.cOptionData.bImage_SaveUse == true &&
            //    //        CMainLib.Ins.cOptionData.bImage_NGSaveUse == true)
            //    //    {
            //    //        BMPFile_Save("FullImage");
            //    //    }
            //    //});

            //    cVisionResultData.bShootFinish = true;
            //}
            //catch (Exception ex)
            //{
            //    NLogger.AddLog(eLogType.CAM1_PipePosture + (int)uiCameraNo, NLogger.eLogLevel.ERROR, ex.ToString(), false);
            //}
        }

        /// <summary>
        /// CAM2 니들 자세 확인
        /// </summary>
        private void CAM2_CheckNeedlePosture()
        {
            try
            {
                // 툴블록 뷰의 화면 선택 콤보 박스에서 0번째 화면의 뷰를 가져와서 Vision 화면에 적용한다.
                if (cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0] != null)
                    cVisionToolBlockUI.CogRecord((CogRecord)cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0]);

                CVisionResultData cVisionResultData = CMainLib.Ins.cVar.GetVisionResultData(uiCameraNo, uiToolBlockNo);
                CogPMAlignTool NeedlePMAlign = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool1"] as CogPMAlignTool;
                CogCaliperTool NeedleCaliper = cCogToolBlockEditV2.Subject.Tools["CogCaliperTool1"] as CogCaliperTool;
                CogFindLineTool CogFindLine1 = cCogToolBlockEditV2.Subject.Tools["CogFindLineTool1"] as CogFindLineTool;
                CogFindLineTool CogFindLine2 = cCogToolBlockEditV2.Subject.Tools["CogFindLineTool2"] as CogFindLineTool;
                bool bNG = false;

                if (NeedlePMAlign.Results != null && NeedleCaliper.Results != null &&
                    CogFindLine1.Results != null && CogFindLine2.Results != null)
                {
                    if (NeedleCaliper.Results.Count >= 2)
                    {
                        double NeedleHeight = NeedleCaliper.Results[0].PositionY - NeedleCaliper.Results[1].PositionY;
                        if (Math.Abs(NeedleHeight) > 180)
                        {
                            GoodNgAddInteractiveGraphics("Fail", 1200, 2600);
                            NLogger.AddLog(eLogType.CAM2_NeedlePosture + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Needle Amount : NG");
                            cVisionResultData.bNeedleDoubleCatch = true;
                            cVisionResultData.bGoodNg = false;
                            bNG = true;
                        }
                        else
                        {
                            cVisionResultData.bNeedleDoubleCatch = false;

                            if (NeedlePMAlign != null && NeedlePMAlign.Results != null)
                            {
                                if (NeedlePMAlign.Results.Count >= 1)
                                {
                                    NLogger.AddLog(eLogType.CAM2_NeedlePosture + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Needle PM Align 180");
                                    cVisionResultData.bGoodNg = false;
                                    bNG = true;
                                }
                                else
                                {
                                    if (CogFindLine1 != null && CogFindLine1.Results.GetLine() == null)
                                    {
                                        GoodNgAddInteractiveGraphics("Fail", 1200, 2600);
                                        NLogger.AddLog(eLogType.CAM2_NeedlePosture + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Needle Height : NG");
                                        cVisionResultData.bGoodNg = false;
                                        bNG = true;
                                    }

                                    if (CogFindLine2 != null && CogFindLine2.Results.GetLine() != null)
                                    {
                                        GoodNgAddInteractiveGraphics("Fail", 1200, 2600);
                                        NLogger.AddLog(eLogType.CAM2_NeedlePosture + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Needle Height : NG");
                                        cVisionResultData.bGoodNg = false;
                                        bNG = true;
                                    }

                                    if (bNG == false)
                                    {
                                        GoodNgAddInteractiveGraphics("OK", 1200, 2600);
                                        cVisionResultData.bGoodNg = true;
                                        NLogger.AddLog(eLogType.CAM2_NeedlePosture + (int)uiCameraNo, NLogger.eLogLevel.INFO, "Needle Posture : OK");
                                    }
                                }
                            }
                        }
                    }

                    Parallel.Invoke(() =>
                    {
                        // 메인 화면에 표시하는 CogDisplay Index 변경, DB 저장
                        if (CMainLib.Ins.McState == eMachineState.RUN)
                        {
                            CMainLib.Ins.SetDBData(uiCameraNo, uiToolBlockNo, true);
                            cVisionToolBlockUI.CogDisplayNextIndex();
                        }
                    });
                    //() =>
                    //{
                    //    // NG Vision 이미지를 Backup 해야할 경우 저장
                    //    if (cVisionResultData.bGoodNg == false &&
                    //        CMainLib.Ins.cOptionData.bImage_SaveUse == true &&
                    //        CMainLib.Ins.cOptionData.bImage_NGSaveUse == true)
                    //    {
                    //        BMPFile_Save("FullImage");
                    //    }
                    //});

                    cVisionResultData.bShootFinish = true;
                }
                else
                {
                    StringBuilder strlog = null;
                    if (NeedlePMAlign.Results == null)
                    {
                        strlog.Append("PMAlign.Results, ");
                    }
                    if (NeedleCaliper.Results == null)
                    {
                        strlog.Append("Caliper.Results, ");
                    }
                    if (CogFindLine1.Results == null)
                    {
                        strlog.Append("FindLine1.Results, ");
                    }
                    if (CogFindLine2.Results == null)
                    {
                        strlog.Append("FindLine2.Results, ");
                    }
                    strlog.Append(" IS NULL");
                    NLogger.AddLog(eLogType.CAM2_NeedlePosture + (int)uiCameraNo, NLogger.eLogLevel.ERROR, strlog.ToString(), false);

                    cVisionResultData.bShootFinish = false;
                }
            }
            catch (Exception ex)
            {
                NLogger.AddLog(eLogType.CAM2_NeedlePosture + (int)uiCameraNo, NLogger.eLogLevel.ERROR, ex.ToString(), false);
            }
        }

        ///// <summary>
        ///// CAM3 파이프를 마운트하기 위해 홀더 촬영
        ///// </summary>
        //private void CAM3_PipeMount()
        //{
        //    try
        //    {
        //        // 툴블록 뷰의 화면 선택 콤보 박스에서 0번째 화면의 뷰를 가져와서 Vision 화면에 적용한다.
        //        if (cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0] != null)
        //            cVisionToolBlockUI.CogRecord((CogRecord)cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0]);

        //        CVisionResultData cVisionResultData = CMainLib.Ins.cVar.GetVisionResultData(uiCameraNo, uiToolBlockNo);
        //        CogPMAlignTool PipeHolderPMAlign1 = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool1"] as CogPMAlignTool;
        //        CogPMAlignTool PipeHolderPMAlign2 = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool2"] as CogPMAlignTool;
        //        CogPMAlignTool PipeHolderPMAlign3 = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool3"] as CogPMAlignTool;
        //        CogPMAlignTool PipeHolderPMAlign4 = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool4"] as CogPMAlignTool;

        //        CogFindCircleTool HolderFindCircle = null;

        //        if (PipeHolderPMAlign1.Results != null && PipeHolderPMAlign2.Results != null &&
        //            PipeHolderPMAlign3.Results != null && PipeHolderPMAlign4.Results != null)
        //        {
        //            // 파이프 마운트 전 홀더유무를 확인한다.
        //            if (PipeHolderPMAlign1.Results.Count == 0)
        //            {
        //                GoodNgAddInteractiveGraphics("Empty", 600, 2200);
        //                NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.INFO, "Holder Empty, Result Fail");
        //                cVisionResultData.bGoodNg = false;
        //                cVisionResultData.bHolderEmpty = true;
        //            }
        //            else
        //            {
        //                CogRectangle cogRect = null;
        //                MapDataLib HolderPipeMap = CMainLib.Ins.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT);
        //                if (HolderPipeMap.GetStatus(eStatus.HOLDER) == false) return;

        //                // 마지막 파이프 삽입 확인 검사
        //                if (cVisionResultData.bLastHoleInsp == true && HolderPipeMap.GetUnitNo(18).eStatus != eStatus.SKIPPED)
        //                {
        //                    cVisionResultData.bLastHoleInsp = false;

        //                    cogRect = new CogRectangle();
        //                    cogRect.SetCenterWidthHeight(CMainLib.Ins.cVisionData.dPipeMatchingPosX[18],
        //                                                 CMainLib.Ins.cVisionData.dPipeMatchingPosY[18],
        //                                                 500,
        //                                                 300);

        //                    PipeHolderPMAlign4.SearchRegion = cogRect;
        //                    PipeHolderPMAlign4.Run();

        //                    if (PipeHolderPMAlign4.Results.Count == 0)
        //                    {
        //                        if (CMainLib.Ins.cVar.bPipeMountInspSkip == false)
        //                        {
        //                            cVisionResultData.bPipeInserFail = true;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    // 파이프 마운트할 니들 홀 No를 가져온다.
        //                    int iMountNo = HolderPipeMap.GetUnitMin(eStatus.HOLDER).iUnitNo;

        //                    // 이전 파이프 삽입 확인 검사
        //                    if (iMountNo > 0 && HolderPipeMap.GetUnitNo(iMountNo - 1).eStatus != eStatus.SKIPPED)
        //                    {
        //                        cogRect = new CogRectangle();
        //                        cogRect.SetCenterWidthHeight(CMainLib.Ins.cVisionData.dPipeMatchingPosX[iMountNo - 1],
        //                                                     CMainLib.Ins.cVisionData.dPipeMatchingPosY[iMountNo - 1],
        //                                                     500,
        //                                                     300);

        //                        PipeHolderPMAlign4.SearchRegion = cogRect;
        //                        PipeHolderPMAlign4.Run();

        //                        if (PipeHolderPMAlign4.Results.Count == 0)
        //                        {
        //                            if (CMainLib.Ins.cVar.bPipeMountInspSkip == false)
        //                            {
        //                                cVisionResultData.bPipeInserFail = true;
        //                            }
        //                        }
        //                    }

        //                    if (cVisionResultData.bPipeInserFail == false)
        //                    {
        //                        // 파이프 삽입 홀 좌표 취득
        //                        cogRect = new CogRectangle();
        //                        cogRect.SetCenterWidthHeight(CMainLib.Ins.cVisionData.dPipeMatchingPosX[iMountNo],
        //                                                     CMainLib.Ins.cVisionData.dPipeMatchingPosY[iMountNo],
        //                                                     500,
        //                                                     300);

        //                        if (iMountNo < 7)
        //                        {
        //                            PipeHolderPMAlign2.SearchRegion = cogRect;
        //                            PipeHolderPMAlign2.Run();

        //                            //파이프 삽입 홀 인식 실패 시
        //                            if (PipeHolderPMAlign2.Results.Count == 0)
        //                            {
        //                                cVisionResultData.bPipeHoleSearchFail = true;
        //                                NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.INFO, $"Pipe Hole No {iMountNo + 1} Vision Hole Not Found Error");
        //                            }

        //                            HolderFindCircle = cCogToolBlockEditV2.Subject.Tools["CogFindCircleTool1"] as CogFindCircleTool;
        //                        }
        //                        else
        //                        {
        //                            PipeHolderPMAlign3.SearchRegion = cogRect;
        //                            PipeHolderPMAlign3.Run();

        //                            //파이프 삽입 홀 인식 실패 시
        //                            if (PipeHolderPMAlign3.Results.Count == 0)
        //                            {
        //                                cVisionResultData.bPipeHoleSearchFail = true;
        //                                NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.INFO, $"Pipe Hole No {iMountNo + 1} Vision Hole Not Found Error");
        //                            }

        //                            HolderFindCircle = cCogToolBlockEditV2.Subject.Tools["CogFindCircleTool2"] as CogFindCircleTool;
        //                            HolderFindCircle.RunParams.ExpectedCircularArc.AngleStart = CMainLib.Ins.cVisionData.dPipeMatchingPosD[iMountNo - 7] * Math.PI / 180;
        //                        }

        //                        HolderFindCircle.Run();
        //                        if (HolderFindCircle.Results != null &&
        //                            HolderFindCircle.Results.GetCircle() != null)
        //                        {
        //                            double dCircleX = HolderFindCircle.Results.GetCircle().CenterX;
        //                            double dCircleY = HolderFindCircle.Results.GetCircle().CenterY;
        //                            double dCalCenterX = CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM3_PipeMount_X_CameraCal;
        //                            double dCalCenterY = CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM3_PipeMount_Y_CameraCal;

        //                            CCamOffset cCamOffset = CMainLib.Ins.cOptionData.GetVisionOffset(uiCameraNo, 0);

        //                            double dXValue = dCircleX - dCalCenterX;
        //                            cVisionResultData.dPipeMountX = Math.Round(dXValue * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);

        //                            double dYValue = dCalCenterY - dCircleY;
        //                            cVisionResultData.dPipeMountY = Math.Round(dYValue * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);
        //                            NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.INFO, $"Result OK : Pipe Hole No : {iMountNo + 1}  X:{cVisionResultData.dPipeMountX} Y:{cVisionResultData.dPipeMountY}");
        //                        }
        //                        else
        //                        {
        //                            cVisionResultData.bGoodNg = false;
        //                            NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.INFO, $"Pipe Hole No {iMountNo + 1} Vision Circle Not Found Error");
        //                        }
        //                    }
        //                }
        //            }

        //            Parallel.Invoke(() =>
        //            {
        //                // 메인 화면에 표시하는 CogDisplay Index 변경, DB 저장
        //                if (CMainLib.Ins.McState == eMachineState.RUN)
        //                {
        //                    CMainLib.Ins.SetDBData(uiCameraNo, uiToolBlockNo, true);
        //                    cVisionToolBlockUI.CogDisplayNextIndex();
        //                }
        //            },
        //            () =>
        //            {
        //                // NG Vision 이미지를 Backup 해야할 경우 저장
        //                if (cVisionResultData.bGoodNg == false &&
        //                    CMainLib.Ins.cOptionData.bImage_SaveUse == true &&
        //                    CMainLib.Ins.cOptionData.bImage_NGSaveUse == true)
        //                {
        //                    BMPFile_Save("FullImage");
        //                }
        //            });

        //            cVisionResultData.bShootFinish = true;
        //        }
        //        else
        //        {
        //            StringBuilder strlog = null;
        //            if (PipeHolderPMAlign1.Results == null)
        //            {
        //                strlog.Append("PMAlign1.Results, ");
        //            }
        //            if (PipeHolderPMAlign2.Results == null)
        //            {
        //                strlog.Append("PMAlign2.Results, ");
        //            }
        //            if (PipeHolderPMAlign3.Results == null)
        //            {
        //                strlog.Append("PMAlign3.Results, ");
        //            }
        //            if (PipeHolderPMAlign4.Results == null)
        //            {
        //                strlog.Append("PMAlign4.Results, ");
        //            }
        //            strlog.Append(" IS NULL");

        //            NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.ERROR, strlog.ToString(), false);

        //            cVisionResultData.bShootFinish = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.ERROR, ex.ToString(), false);
        //    }
        //}

        /// <summary>
        /// CAM3 첫번째 니들을 마운트하기 위해 니들이 장착된 홀더 촬영
        /// </summary>
        private void CAM3_PipeMount()
        {
            try
            {
                // 툴블록 뷰의 화면 선택 콤보 박스에서 0번째 화면의 뷰를 가져와서 Vision 화면에 적용한다.
                if (cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0] != null)
                    cVisionToolBlockUI.CogRecord((CogRecord)cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0]);

                CVisionResultData cVisionResultData = CMainLib.Ins.cVar.GetVisionResultData(uiCameraNo, uiToolBlockNo);
                CogPMAlignTool FirstNeedleHolderPMAlign1 = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool1"] as CogPMAlignTool;
                CogPMRedLineTool FirstNeedleHolderPMRedLine1 = cCogToolBlockEditV2.Subject.Tools["CogPMRedLineTool1"] as CogPMRedLineTool;
                CogPMAlignTool FirstNeedleHolderPMAlign3 = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool3"] as CogPMAlignTool;
                CogFindCircleTool FirstHolderFindCircle = cCogToolBlockEditV2.Subject.Tools["CogFindCircleTool1"] as CogFindCircleTool;

                if (FirstNeedleHolderPMAlign1.Results != null && FirstNeedleHolderPMRedLine1.Results != null &&
                    FirstNeedleHolderPMAlign3.Results != null && FirstHolderFindCircle.Results != null)
                {
                    // 첫번째 니들 마운트 전 홀더유무를 확인한다.
                    if (FirstNeedleHolderPMAlign1.Results.Count == 0)
                    {
                        GoodNgAddInteractiveGraphics("Empty", 600, 2200);
                        NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.INFO, "Holder Empty, Result Fail");
                        cVisionResultData.bGoodNg = false;
                        cVisionResultData.bHolderEmpty = true;
                    }
                    else
                    {
                        CogRectangle cogRect = null;
                        MapDataLib HolderPipeMap = CMainLib.Ins.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT);
                        if (HolderPipeMap.GetStatus(eStatus.FIRST_HOLE) == false) return;

                        ////마지막 홀이라면 삽입 확인 검사 플래그 ON
                        //if (iMountNo == 18)
                        //{
                        //    cVisionResultData.bLastHoleInsp = true;
                        //}

                        // 마지막 파이프 삽입 확인 검사
                        if (cVisionResultData.bLastHoleInsp == true)
                        {
                            cVisionResultData.bLastHoleInsp = false;
                            cogRect = new CogRectangle();
                            cogRect.SetCenterWidthHeight(CMainLib.Ins.cVisionData.dPipeMatchingPosX[18],
                                                         CMainLib.Ins.cVisionData.dPipeMatchingPosY[18],
                                                         500,
                                                         300);
                            FirstNeedleHolderPMAlign3.SearchRegion = cogRect;
                            FirstNeedleHolderPMAlign3.Run();

                            if (FirstNeedleHolderPMAlign3.Results.Count == 0)
                            {
                                if (CMainLib.Ins.cVar.bPipeMountInspSkip == false)
                                    cVisionResultData.bPipeInserFail = true;
                            }
                        }
                        else
                        {
                            // 첫번째 니들 마운트할 홀 No를 가져온다.
                            int iMountNo = HolderPipeMap.GetUnitMin(eStatus.FIRST_HOLE).iUnitNo;

                            // 첫번째 니들 삽입 검사 홀 No를 가져온다.
                            int iInspNo = -1;
                            for (int i = iMountNo - 1; i >= 0; i--)
                            {
                                if (CMainLib.Ins.cSysOne.bHolderNeedlePinWorkCount[i] == (int)eNeedlePinStatus.Firstwork)
                                {
                                    iInspNo = i;
                                    break;
                                }
                            }

                            // 이전 니들 삽입 확인 검사
                            if (iInspNo != -1 && HolderPipeMap.GetUnitNo(iInspNo).eStatus != eStatus.SKIPPED)
                            {
                                cogRect = new CogRectangle();
                                cogRect.SetCenterWidthHeight(CMainLib.Ins.cVisionData.dNeedleMatchingPosX[iInspNo],
                                                             CMainLib.Ins.cVisionData.dNeedleMatchingPosY[iInspNo],
                                                             500,
                                                             300);
                                FirstNeedleHolderPMAlign3.SearchRegion = cogRect;
                                FirstNeedleHolderPMAlign3.Run();

                                if (FirstNeedleHolderPMAlign3.Results.Count == 0)
                                {
                                    //if (CMainLib.Ins.cVar.bPipeMountInspSkip == false)
                                    //cVisionResultData.bPipeInserFail = true;
                                }
                            }

                            // 니들 삽입 홀 좌표 취득
                            if (cVisionResultData.bNeedleInserFail == false)
                            {
                                cogRect = new CogRectangle();
                                cogRect.SetCenterWidthHeight(CMainLib.Ins.cVisionData.dNeedleMatchingPosX[iMountNo],
                                                             CMainLib.Ins.cVisionData.dNeedleMatchingPosY[iMountNo],
                                                             500,
                                                             300);
                                NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.INFO, $"First Needle Hole No {iMountNo + 1} X : {CMainLib.Ins.cVisionData.dNeedleMatchingPosX[iMountNo]} Y : { CMainLib.Ins.cVisionData.dNeedleMatchingPosY[iMountNo]} ");

                                FirstNeedleHolderPMRedLine1.SearchRegion = cogRect;

                                FirstNeedleHolderPMRedLine1.Run();

                                //니들 삽입 홀 인식 실패 시
                                if (FirstNeedleHolderPMRedLine1.Results.Count == 0)
                                {
                                    cVisionResultData.bNeedleHoleSearchFail = true;
                                    NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.INFO, $"First Needle Hole No {iMountNo + 1} Vision Hole Not Found Error");
                                }

                                double dRedLineCenterX = FirstNeedleHolderPMRedLine1.Results[0].GetPose().TranslationX;
                                double dRedLineCenterY = FirstNeedleHolderPMRedLine1.Results[0].GetPose().TranslationY;

                                FirstHolderFindCircle.Run();

                                if (FirstHolderFindCircle.Results.GetCircle() != null)
                                {
                                    double dCircleX = FirstHolderFindCircle.Results.GetCircle().CenterX;
                                    double dCircleY = FirstHolderFindCircle.Results.GetCircle().CenterY;
                                    double dCalCenterX = CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM3_PipeMount_X_CameraCal;
                                    double dCalCenterY = CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM3_PipeMount_Y_CameraCal;

                                    bool A = dRedLineCenterX - 20 > dCircleX;
                                    bool B = dCircleX > dRedLineCenterX + 20;
                                    bool C = dRedLineCenterY - 20 > dCircleY;
                                    bool D = dCircleY > dRedLineCenterY + 20;

                                    if (A || B || C || D == true)
                                    {
                                        dCircleX = dRedLineCenterX;
                                        dCircleY = dRedLineCenterY;
                                    }

                                    CCamOffset cCamOffset = CMainLib.Ins.cOptionData.GetVisionOffset(uiCameraNo, 0);

                                    double dXValue = dCircleX - dCalCenterX;
                                    cVisionResultData.dNeedleMountX = Math.Round(dXValue * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);

                                    double dYValue = dCalCenterY - dCircleY;
                                    cVisionResultData.dNeedleMountY = Math.Round(dYValue * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);
                                    NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.INFO, $"Result OK : First Needle Hole No : {iMountNo + 1} X:{cVisionResultData.dNeedleMountX} Y:{cVisionResultData.dNeedleMountY}");
                                }
                                else if (CMainLib.Ins.cVar.bForcedNeedleMount == true)
                                {
                                    double dCircleX = dRedLineCenterX;
                                    double dCircleY = dRedLineCenterY;
                                    double dCalCenterX = CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM3_PipeMount_X_CameraCal;
                                    double dCalCenterY = CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM3_PipeMount_Y_CameraCal;

                                    CCamOffset cCamOffset = CMainLib.Ins.cOptionData.GetVisionOffset(uiCameraNo, 0);

                                    double dXValue = dCircleX - dCalCenterX;
                                    cVisionResultData.dNeedleMountX = Math.Round(dXValue * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);

                                    double dYValue = dCalCenterY - dCircleY;
                                    cVisionResultData.dNeedleMountY = Math.Round(dYValue * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);
                                    NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.INFO, $"Result OK : First Needle Hole No : {iMountNo + 1} X:{cVisionResultData.dNeedleMountX} Y:{cVisionResultData.dNeedleMountY}");
                                }
                                else
                                {
                                    cVisionResultData.bGoodNg = false;
                                    NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.INFO, $"First Needle Hole No {iMountNo + 1} Vision Circle Not Found Error");
                                }
                            }
                        }
                    }

                    Parallel.Invoke(() =>
                    {
                        // 메인 화면에 표시하는 CogDisplay Index 변경, DB 저장
                        if (CMainLib.Ins.McState == eMachineState.RUN)
                        {
                            CMainLib.Ins.SetDBData(uiCameraNo, uiToolBlockNo, true);
                            cVisionToolBlockUI.CogDisplayNextIndex();
                        }
                    },
                    () =>
                    {
                        // NG Vision 이미지를 Backup 해야할 경우 저장
                        if (cVisionResultData.bGoodNg == false &&
                                CMainLib.Ins.cOptionData.bImage_SaveUse == true &&
                                CMainLib.Ins.cOptionData.bImage_NGSaveUse == true)
                        {
                            BMPFile_Save("FullImage");
                        }
                    });

                    cVisionResultData.bShootFinish = true;
                }
                else
                {
                    StringBuilder strlog = null;
                    if (FirstNeedleHolderPMAlign1.Results == null)
                    {
                        strlog.Append("PMAlign1.Results, ");
                    }
                    if (FirstNeedleHolderPMAlign3.Results == null)
                    {
                        strlog.Append("PMAlign3.Results, ");
                    }
                    if (FirstNeedleHolderPMRedLine1.Results == null)
                    {
                        strlog.Append("PMRedLine1.Results, ");
                    }
                    if (FirstHolderFindCircle.Results == null)
                    {
                        strlog.Append("FindCircle.Results, ");
                    }
                    strlog.Append(" IS NULL");

                    NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.ERROR, strlog.ToString(), false);

                    cVisionResultData.bShootFinish = false;
                }
            }
            catch (Exception ex)
            {
                NLogger.AddLog(eLogType.CAM3_PipeMount, NLogger.eLogLevel.ERROR, ex.ToString(), false);
            }
        }

        /// <summary>
        /// CAM4 니들을 마운트하기 위해 파이프가 장착된 홀더 촬영
        /// </summary>
        private void CAM4_NeedleMount()
        {
            try
            {
                // 툴블록 뷰의 화면 선택 콤보 박스에서 0번째 화면의 뷰를 가져와서 Vision 화면에 적용한다.
                if (cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0] != null)
                    cVisionToolBlockUI.CogRecord((CogRecord)cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0]);

                CVisionResultData cVisionResultData = CMainLib.Ins.cVar.GetVisionResultData(uiCameraNo, uiToolBlockNo);
                CogPMAlignTool NeedleHolderPMAlign1 = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool1"] as CogPMAlignTool;
                CogPMRedLineTool NeedleHolderPMRedLine1 = cCogToolBlockEditV2.Subject.Tools["CogPMRedLineTool1"] as CogPMRedLineTool;
                CogPMAlignTool NeedleHolderPMAlign3 = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool3"] as CogPMAlignTool;
                CogFindCircleTool HolderFindCircle = cCogToolBlockEditV2.Subject.Tools["CogFindCircleTool1"] as CogFindCircleTool;

                if (NeedleHolderPMAlign1.Results != null && NeedleHolderPMRedLine1.Results != null &&
                    NeedleHolderPMAlign3.Results != null && HolderFindCircle.Results != null)
                {
                    // 니들 마운트 전 홀더유무를 확인한다.
                    if (NeedleHolderPMAlign1.Results.Count == 0)
                    {
                        GoodNgAddInteractiveGraphics("Empty", 600, 2200);
                        NLogger.AddLog(eLogType.CAM4_NeedleMount, NLogger.eLogLevel.INFO, "Holder Empty, Result Fail");
                        cVisionResultData.bGoodNg = false;
                        cVisionResultData.bHolderEmpty = true;
                    }
                    else
                    {
                        CogRectangle cogRect = null;
                        MapDataLib HolderNeedleMap = CMainLib.Ins.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT);
                        if (HolderNeedleMap.GetStatus(eStatus.HOLDER) == false) return;

                        ////마지막 홀이라면 삽입 확인 검사 플래그 ON
                        //if (iMountNo == 18)
                        //{
                        //    cVisionResultData.bLastHoleInsp = true;
                        //}

                        // 마지막 니들 삽입 확인 검사
                        if (cVisionResultData.bLastHoleInsp == true)
                        {
                            cVisionResultData.bLastHoleInsp = false;
                            cogRect = new CogRectangle();
                            cogRect.SetCenterWidthHeight(CMainLib.Ins.cVisionData.dNeedleMatchingPosX[18],
                                                         CMainLib.Ins.cVisionData.dNeedleMatchingPosY[18],
                                                         500,
                                                         300);
                            NeedleHolderPMAlign3.SearchRegion = cogRect;
                            NeedleHolderPMAlign3.Run();

                            if (NeedleHolderPMAlign3.Results.Count == 0)
                            {
                                if (CMainLib.Ins.cVar.bNeedleMountInspSkip == false)
                                    cVisionResultData.bNeedleInserFail = true;
                            }
                        }
                        else
                        {
                            // 니들 마운트할 니들 홀 No를 가져온다.
                            int iMountNo = HolderNeedleMap.GetUnitMin(eStatus.HOLDER).iUnitNo;

                            // 니들 삽입 검사 홀 No를 가져온다.
                            int iInspNo = -1;
                            for (int i = iMountNo - 1; i >= 0; i--)
                            {
                                if (CMainLib.Ins.cSysOne.bHolderNeedlePinWorkCount[i] == (int)eNeedlePinStatus.Secondwork)
                                {
                                    iInspNo = i;
                                    break;
                                }
                            }

                            // 이전 니들 삽입 확인 검사
                            if (iInspNo != -1 && HolderNeedleMap.GetUnitNo(iInspNo).eStatus != eStatus.SKIPPED)
                            {
                                cogRect = new CogRectangle();
                                cogRect.SetCenterWidthHeight(CMainLib.Ins.cVisionData.dNeedleMatchingPosX[iInspNo],
                                                             CMainLib.Ins.cVisionData.dNeedleMatchingPosY[iInspNo],
                                                             500,
                                                             300);
                                NeedleHolderPMAlign3.SearchRegion = cogRect;
                                NeedleHolderPMAlign3.Run();

                                if (NeedleHolderPMAlign3.Results.Count == 0)
                                {
                                    //if (CMainLib.Ins.cVar.bNeedleMountInspSkip == false)
                                    //cVisionResultData.bNeedleInserFail = true;
                                }
                            }

                            // 니들 삽입 홀 좌표 취득
                            if (cVisionResultData.bNeedleInserFail == false)
                            {
                                cogRect = new CogRectangle();
                                cogRect.SetCenterWidthHeight(CMainLib.Ins.cVisionData.dNeedleMatchingPosX[iMountNo],
                                                             CMainLib.Ins.cVisionData.dNeedleMatchingPosY[iMountNo],
                                                             500,
                                                             300);
                                NLogger.AddLog(eLogType.CAM4_NeedleMount, NLogger.eLogLevel.INFO, $"Needle Hole No {iMountNo + 1} X : {CMainLib.Ins.cVisionData.dNeedleMatchingPosX[iMountNo]} Y : { CMainLib.Ins.cVisionData.dNeedleMatchingPosY[iMountNo]} ");

                                NeedleHolderPMRedLine1.SearchRegion = cogRect;

                                NeedleHolderPMRedLine1.Run();

                                //니들 삽입 홀 인식 실패 시
                                if (NeedleHolderPMRedLine1.Results.Count == 0)
                                {
                                    cVisionResultData.bNeedleHoleSearchFail = true;
                                    NLogger.AddLog(eLogType.CAM4_NeedleMount, NLogger.eLogLevel.INFO, $"Needle Hole No {iMountNo + 1} Vision Hole Not Found Error");
                                }

                                double dRedLineCenterX = NeedleHolderPMRedLine1.Results[0].GetPose().TranslationX;
                                double dRedLineCenterY = NeedleHolderPMRedLine1.Results[0].GetPose().TranslationY;

                                HolderFindCircle.Run();

                                if (HolderFindCircle.Results.GetCircle() != null)
                                {
                                    double dCircleX = HolderFindCircle.Results.GetCircle().CenterX;
                                    double dCircleY = HolderFindCircle.Results.GetCircle().CenterY;
                                    double dCalCenterX = CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM4_NeedleMount_X_CameraCal;
                                    double dCalCenterY = CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM4_NeedleMount_Y_CameraCal;

                                    bool A = dRedLineCenterX - 20 > dCircleX;
                                    bool B = dCircleX > dRedLineCenterX + 20;
                                    bool C = dRedLineCenterY - 20 > dCircleY;
                                    bool D = dCircleY > dRedLineCenterY + 20;

                                    if (A || B || C || D == true)
                                    {
                                        dCircleX = dRedLineCenterX;
                                        dCircleY = dRedLineCenterY;
                                    }

                                    CCamOffset cCamOffset = CMainLib.Ins.cOptionData.GetVisionOffset(uiCameraNo, 0);

                                    double dXValue = dCircleX - dCalCenterX;
                                    cVisionResultData.dNeedleMountX = Math.Round(dXValue * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);

                                    double dYValue = dCalCenterY - dCircleY;
                                    cVisionResultData.dNeedleMountY = Math.Round(dYValue * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);
                                    NLogger.AddLog(eLogType.CAM4_NeedleMount, NLogger.eLogLevel.INFO, $"Result OK : Needle Hole No : {iMountNo + 1} X:{cVisionResultData.dNeedleMountX} Y:{cVisionResultData.dNeedleMountY}");
                                }
                                else if (CMainLib.Ins.cVar.bForcedNeedleMount == true)
                                {
                                    double dCircleX = dRedLineCenterX;
                                    double dCircleY = dRedLineCenterY;
                                    double dCalCenterX = CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM4_NeedleMount_X_CameraCal;
                                    double dCalCenterY = CMainLib.Ins.cSysParamCollData.GetSysArray().dCAM4_NeedleMount_Y_CameraCal;

                                    CCamOffset cCamOffset = CMainLib.Ins.cOptionData.GetVisionOffset(uiCameraNo, 0);

                                    double dXValue = dCircleX - dCalCenterX;
                                    cVisionResultData.dNeedleMountX = Math.Round(dXValue * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);

                                    double dYValue = dCalCenterY - dCircleY;
                                    cVisionResultData.dNeedleMountY = Math.Round(dYValue * CMainLib.Ins.cVisionData.dResolution[uiCameraNo], 3);
                                    NLogger.AddLog(eLogType.CAM4_NeedleMount, NLogger.eLogLevel.INFO, $"Result OK : Needle Hole No : {iMountNo + 1} X:{cVisionResultData.dNeedleMountX} Y:{cVisionResultData.dNeedleMountY}");
                                }
                                else
                                {
                                    cVisionResultData.bGoodNg = false;
                                    NLogger.AddLog(eLogType.CAM4_NeedleMount, NLogger.eLogLevel.INFO, $"Needle Hole No {iMountNo + 1} Vision Circle Not Found Error");
                                }
                            }
                        }
                    }

                    Parallel.Invoke(() =>
                    {
                        // 메인 화면에 표시하는 CogDisplay Index 변경, DB 저장
                        if (CMainLib.Ins.McState == eMachineState.RUN)
                        {
                            CMainLib.Ins.SetDBData(uiCameraNo, uiToolBlockNo, true);
                            cVisionToolBlockUI.CogDisplayNextIndex();
                        }
                    },
                    () =>
                    {
                        // NG Vision 이미지를 Backup 해야할 경우 저장
                        if (cVisionResultData.bGoodNg == false &&
                                CMainLib.Ins.cOptionData.bImage_SaveUse == true &&
                                CMainLib.Ins.cOptionData.bImage_NGSaveUse == true)
                        {
                            BMPFile_Save("FullImage");
                        }
                    });

                    cVisionResultData.bShootFinish = true;
                }
                else
                {
                    StringBuilder strlog = null;
                    if (NeedleHolderPMAlign1.Results == null)
                    {
                        strlog.Append("PMAlign1.Results, ");
                    }
                    if (NeedleHolderPMAlign3.Results == null)
                    {
                        strlog.Append("PMAlign3.Results, ");
                    }
                    if (NeedleHolderPMRedLine1.Results == null)
                    {
                        strlog.Append("PMRedLine1.Results, ");
                    }
                    if (HolderFindCircle.Results == null)
                    {
                        strlog.Append("FindCircle.Results, ");
                    }
                    strlog.Append(" IS NULL");

                    NLogger.AddLog(eLogType.CAM4_NeedleMount, NLogger.eLogLevel.ERROR, strlog.ToString(), false);

                    cVisionResultData.bShootFinish = false;
                }
            }
            catch (Exception ex)
            {
                NLogger.AddLog(eLogType.CAM4_NeedleMount, NLogger.eLogLevel.ERROR, ex.ToString(), false);
            }
        }

        /// <summary>
        /// 파이프와 니들을 장착한 홀더에 본딩을 하기위해 얼라인 촬영
        /// </summary>
        private void CAM5_DispenserAlign()
        {
            try
            {
                // 툴블록 뷰의 화면 선택 콤보 박스에서 0번째 화면의 뷰를 가져와서 Vision 화면에 적용한다.
                if (cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0] != null)
                    cVisionToolBlockUI.CogRecord((CogRecord)cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0]);

                CVisionResultData cVisionResultData = CMainLib.Ins.cVar.GetVisionResultData(uiCameraNo, uiToolBlockNo);
                CogPMAlignTool HoleFindPMAlign1 = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool1"] as CogPMAlignTool;
                CogPMAlignTool HoleFindPMAlign2 = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool2"] as CogPMAlignTool;
                CogPMAlignTool HoleFindPMAlign3 = cCogToolBlockEditV2.Subject.Tools["CogPMAlignTool3"] as CogPMAlignTool;
                if (HoleFindPMAlign1 != null && HoleFindPMAlign1.Results != null &&
                    HoleFindPMAlign2 != null && HoleFindPMAlign2.Results != null &&
                    HoleFindPMAlign3 != null && HoleFindPMAlign3.Results != null)  // 결과가 있으면

                {
                    // 홀더 유무 확인
                    if (HoleFindPMAlign1.Results.Count == 0)
                    {
                        GoodNgAddInteractiveGraphics("Empty", 700, 2900);
                        NLogger.AddLog(eLogType.CAM5_Dispenser, NLogger.eLogLevel.INFO, "Holder Empty, Result Fail");
                        cVisionResultData.bGoodNg = false;
                        cVisionResultData.bHolderEmpty = true;
                    }
                    // 홀더 자세 확인
                    else if (HoleFindPMAlign2.Results.Count == 0)
                    {
                        GoodNgAddInteractiveGraphics("Position Fail", 700, 2900);
                        NLogger.AddLog(eLogType.CAM5_Dispenser, NLogger.eLogLevel.INFO, "Holder Empty, Result Fail");
                        cVisionResultData.bGoodNg = false;
                        cVisionResultData.bHolderPositionFail = true;
                    }
                    // 홀더 아래 깔린 자재 확인
                    else if (HoleFindPMAlign3.Results.Count > 0)
                    {
                        GoodNgAddInteractiveGraphics("Position Fail", 700, 2900);
                        NLogger.AddLog(eLogType.CAM5_Dispenser, NLogger.eLogLevel.INFO, "Other Needle Exist, Result Fail");
                        cVisionResultData.bGoodNg = false;
                        cVisionResultData.bHolderPositionFail = true;
                    }
                    else
                    {
                        cVisionResultData.bGoodNg = true;
                    }

                    //// 박힌 니들 갯수 확인
                    //else
                    //{
                    //    int iFailCount = 0;
                    //    for (int i = 0; i < CMainLib.Ins.cSysOne.bHolderNeedlePinWorkCount.Length; i++)
                    //    {
                    //        if (CMainLib.Ins.cSysOne.bHolderNeedlePinWorkCount[i] == false)
                    //            iFailCount++;
                    //    }

                    //    int iNeedleNumberToFind = 19 - iFailCount;
                    //    HolderFindPMRedLineTool1.RunParams.NumberToFind = iNeedleNumberToFind;
                    //    // 홀 검사 스킵
                    //    if (CMainLib.Ins.cOptionData.bHolderInspSkip == true)
                    //    {
                    //        GoodNgAddInteractiveGraphics("Skip", 700, 2900);
                    //        NLogger.AddLog(eLogType.CAM5_Dispenser, NLogger.eLogLevel.INFO, "Insp Skip");
                    //        cVisionResultData.bGoodNg = true;
                    //    }
                    //    // 니들 홀 검출
                    //    else if (HolderFindPMRedLineTool1.Results.Count != iNeedleNumberToFind)
                    //    {
                    //        GoodNgAddInteractiveGraphics("Needle Hole", 700, 2900);
                    //        NLogger.AddLog(eLogType.CAM5_Dispenser, NLogger.eLogLevel.INFO, "Needle Hole Find, Result Fail");
                    //        cVisionResultData.bGoodNg = false;
                    //    }
                    //    else  // 검출 안됨
                    //    {
                    //        GoodNgAddInteractiveGraphics("OK", 700, 2900);
                    //        NLogger.AddLog(eLogType.CAM5_Dispenser, NLogger.eLogLevel.INFO, "Result Good");
                    //        cVisionResultData.bGoodNg = true;
                    //    }
                    //}

                    Parallel.Invoke(() =>
                    {
                        // 메인 화면에 표시하는 CogDisplay Index 변경, DB 저장
                        if (CMainLib.Ins.McState == eMachineState.RUN)
                        {
                            CMainLib.Ins.SetDBData(uiCameraNo, uiToolBlockNo, true);
                            cVisionToolBlockUI.CogDisplayNextIndex();
                        }
                    },
                    () =>
                    {
                        // NG Vision 이미지를 Backup 해야할 경우 저장
                        if (cVisionResultData.bGoodNg == false &&
                                CMainLib.Ins.cOptionData.bImage_SaveUse == true &&
                                CMainLib.Ins.cOptionData.bImage_NGSaveUse == true)
                        {
                            BMPFile_Save("FullImage");
                        }
                    });

                    cVisionResultData.bShootFinish = true;
                }
                else
                {
                    StringBuilder strlog = null;

                    if (HoleFindPMAlign1 == null)
                    {
                        strlog.Append("PMAlign1, ");
                    }
                    if (HoleFindPMAlign2 == null)
                    {
                        strlog.Append("PMAlig2, ");
                    }
                    if (HoleFindPMAlign3 == null)
                    {
                        strlog.Append("PMAlign3, ");
                    }
                    if (HoleFindPMAlign1.Results == null)
                    {
                        strlog.Append("PMAlign1.Results, ");
                    }
                    if (HoleFindPMAlign2.Results == null)
                    {
                        strlog.Append("PMAlig2.Results, ");
                    }
                    if (HoleFindPMAlign3.Results == null)
                    {
                        strlog.Append("PMAlign3.Results, ");
                    }
                    strlog.Append(" IS NULL");

                    NLogger.AddLog(eLogType.CAM5_Dispenser + (int)uiCameraNo, NLogger.eLogLevel.ERROR, strlog.ToString(), false);

                    cVisionResultData.bShootFinish = false;
                }
            }
            catch (Exception ex)
            {
                NLogger.AddLog(eLogType.CAM5_Dispenser + (int)uiCameraNo, NLogger.eLogLevel.ERROR, ex.ToString(), false);
            }
        }

        /// <summary>
        /// 기타 카메라 검사
        /// </summary>
        private void Else_CAM()
        {
            bool bGoodNg = false;
            string strTotalData = string.Empty;

            try
            {
                // 툴블록 뷰의 화면 선택 콤보 박스에서 0번째 화면의 뷰를 가져와서 Vision 화면에 적용한다.
                if (cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0] != null)
                    cVisionToolBlockUI.CogRecord((CogRecord)cCogToolBlockEditV2.Subject.CreateLastRunRecord().SubRecords[0]);
                CogPMRedLineTool cogPMRedLineTool = cCogToolBlockEditV2.Subject.Tools["CogPMRedLineTool1"] as CogPMRedLineTool;
                if (cogPMRedLineTool.Results != null && cogPMRedLineTool.Results.Count > 0)  // 결과가 하나라도 있으면
                {
                    // 픽셀 위치 값 배율 계산으로 실 거리 값 변환
                    CVisionResult cResult = PixelToMilimeter(cogPMRedLineTool.Results[0].GetPose());
                    // Offset 값 적용
                    cResult = AddCamOffset(cResult);
                    strTotalData += cResult.ToString();
                    string strResult = "X[" + cResult.dX.ToString() + "] Y[" + cResult.dY.ToString() + "] T[" + cResult.dT.ToString() + "]";
                    ResultAddInteractiveGraphics(strResult, 2000, 1850);
                    // GOOD, NG or NOT 설정
                    GoodNgAddInteractiveGraphics("OK", 250, 1850);
                    bGoodNg = true;
                }
                else  // 검출 안됨
                {
                    GoodNgAddInteractiveGraphics("NOT", 250, 1850);
                }

                CMainLib.Ins.VisionResultMessage(uiCameraNo, uiToolBlockNo, strTotalData, bGoodNg);

                Parallel.Invoke(() =>
                {
                    // 메인 화면에 표시하는 CogDisplay Index 변경, DB 저장
                    if (CMainLib.Ins.McState == eMachineState.RUN)
                    {
                        CMainLib.Ins.SetDBData(uiCameraNo, uiToolBlockNo, true);
                        cVisionToolBlockUI.CogDisplayNextIndex();
                    }
                },
                () =>
                {
                    // NG Vision 이미지를 Backup 해야할 경우 저장
                    if (bGoodNg == false &&
                        CMainLib.Ins.cOptionData.bImage_SaveUse == true &&
                        CMainLib.Ins.cOptionData.bImage_NGSaveUse == true)
                    {
                        BMPFile_Save("FullImage");
                    }
                });
            }
            catch (Exception ex)
            {
                NLogger.AddLog(eLogType.CAM0_PipeNeedlePickUp + (int)uiCameraNo, NLogger.eLogLevel.ERROR, ex.ToString(), false);
            }
        }

        /// <summary>
        /// ToolBlock vpp파일 오픈 시 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void cogToolBlockEditV21_SubjectChanged(object sender, EventArgs e)
        {
            // The application is meant to be used with the TB.vpp so whenever the user changes the TB
            // We disable the run once button
        }
    }

    /// <summary>
    /// Vision 기능 관리 클래스
    /// </summary>
    public class CVisionToolBlockLib
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public CVisionToolBlockLib()
        { }

        /// <summary>
        /// cAcqFifoTool이 연결됐는지 확인 플래그
        /// </summary>
        public bool iAcqConnnection = false;

        /// <summary>
        /// 촬영 진행 상태
        /// </summary>
        public bool bRun = false;

        /// <summary>
        /// 코그넥스 카메라 설정 관리 클래스
        /// </summary>
        public CogAcqFifoTool cAcqFifoTool = null;

        /// <summary>
        /// 코그넥스 CogToolBlockClass 리스트 클래스
        /// </summary>
        public List<CogToolBlockClass> cCogToolBlockList = new List<CogToolBlockClass>();

        /// <summary>
        /// Vision UI 클래스
        /// </summary>
        private VisionToolBlockUI cVisionToolBlockUI = null;

        /// <summary>
        /// Camera 번호
        /// </summary>
        private uint uiCameraNo = 0;

        /// <summary>
        /// Tool Block 번호
        /// </summary>
        public uint uiToolBlockNo = 0;

        /// <summary>
        /// 초기화 함수
        /// </summary>
        /// <param name="cVisionToolBlockUI"></param>
        /// <param name="uiCameraNo"></param>
        /// <param name="bVisionLicense"></param>
        public void Init(VisionToolBlockUI cVisionToolBlockUI, uint uiCameraNo, bool bVisionLicense)
        {
            this.cVisionToolBlockUI = cVisionToolBlockUI;
            this.uiCameraNo = uiCameraNo;

            // ToolBlock 필요한 개 수만큼 생성
            for (int i = 0; i < CMainLib.Ins.cVisionData.strToolBlockName[uiCameraNo].Length; i++)
            {
                CogToolBlockClass cCogToolBlockClass = new CogToolBlockClass();
                cCogToolBlockClass.uiCameraNo = uiCameraNo;
                cCogToolBlockClass.uiToolBlockNo = (uint)i;
                cCogToolBlockClass.cVisionToolBlockUI = cVisionToolBlockUI;
                cCogToolBlockList.Add(cCogToolBlockClass);
            }

            cVisionToolBlockUI.Init(this, uiCameraNo);
            if (bVisionLicense == true) VisionLoad();
        }

        /// <summary>
        /// Cognex Vision Load
        /// </summary>
        private void VisionLoad()
        {
            cAcqFifoTool = (CogAcqFifoTool)CogSerializer.LoadObjectFromFile(Get_VppPath(eVppName.AcqFifo, uiCameraNo));
            cAcqFifoTool.Ran += AcqFifoTool_Ran;
            if (cAcqFifoTool.Operator != null) iAcqConnnection = true;

            foreach (CogToolBlockClass cCogToolBlockClass in cCogToolBlockList)
            {
                cCogToolBlockClass.cImageFileTool.Ran += new EventHandler(cCogToolBlockClass.ImageFileTool_Ran);
                cCogToolBlockClass.cCogToolBlockEditV2.Subject = CogSerializer.LoadObjectFromFile(
                                                                 Get_VppPath(eVppName.ToolBlock, uiCameraNo, cCogToolBlockClass.uiToolBlockNo)) as CogToolBlock;

                cCogToolBlockClass.cCogToolBlockEditV2.Subject.Ran += new EventHandler(cCogToolBlockClass.ToolBlock_Ran);
                cCogToolBlockClass.cCogToolBlockEditV2.SubjectChanged += new EventHandler(cCogToolBlockClass.cogToolBlockEditV21_SubjectChanged);
                cCogToolBlockClass.bCogInitialize = true;
            }
        }

        /// <summary>
        /// Cognex Vision Close
        /// </summary>
        public void VisionClose()
        {
            if (Define.SIMULATION == false)
            {
                iAcqConnnection = false;
                // Vision 해제
                cAcqFifoTool.Ran -= AcqFifoTool_Ran;
                if (cAcqFifoTool != null) cAcqFifoTool.Dispose();
                foreach (CogToolBlockClass cCogToolBlockClass in cCogToolBlockList)
                {
                    cCogToolBlockClass.cImageFileTool.Ran -= new EventHandler(cCogToolBlockClass.ImageFileTool_Ran);
                    if (cCogToolBlockClass.cImageFileTool != null) cCogToolBlockClass.cImageFileTool.Dispose();
                    cCogToolBlockClass.cCogToolBlockEditV2.Subject.Ran -= new EventHandler(cCogToolBlockClass.ToolBlock_Ran);
                    cCogToolBlockClass.cCogToolBlockEditV2.SubjectChanged -= new EventHandler(cCogToolBlockClass.cogToolBlockEditV21_SubjectChanged);
                    if (cCogToolBlockClass.cCogToolBlockEditV2 != null) cCogToolBlockClass.cCogToolBlockEditV2.Dispose();
                    cCogToolBlockClass.bCogInitialize = false;
                }
            }
        }

        /// <summary>
        /// VPP 파일명 생성
        /// </summary>
        /// <param name="eVppName"></param>
        /// <param name="uiCameraNo"></param>
        /// <param name="iIndex"></param>
        /// <returns></returns>
        public string Get_VppPath(eVppName eVppName, uint uiCameraNo, uint iIndex = 0)
        {
            string strReturn = "";
            string strPath = CXMLProcess.GetVisionModelPath(CMainLib.Ins.cSysOne.uiCurrentModelNo);

            switch (eVppName)
            {
                case eVppName.AcqFifo:
                    strReturn = String.Format("{0}AcqFifo_CAM{1}.vpp", strPath, uiCameraNo.ToString("D1"));
                    break;

                case eVppName.ImageFile:
                    strReturn = String.Format("{0}ImageFile_CAM{1}.vpp", strPath, uiCameraNo.ToString("D1"));
                    break;

                case eVppName.ToolBlock:
                    strReturn = String.Format("{0}ToolBlock_CAM{1}_{2}.vpp", strPath, uiCameraNo.ToString("D1"),
                                CMainLib.Ins.cVisionData.strToolBlockName[uiCameraNo][iIndex]);
                    break;
            }
            return strReturn;
        }

        /// <summary>
        /// Trigger Mode로 설정을 제어
        /// 촬영하지 않을 때 Trigger 신호 사용 제어 및 라이브 기능 때 트리거 종료
        /// </summary>
        /// <param name="bEnable"></param>
        public void TriggerEnable(bool bEnable)
        {
            try
            {
                ICogAcqTrigger triggerParams = cAcqFifoTool.Operator.OwnedTriggerParams;
                if (triggerParams != null)
                {
                    if (bEnable == true) triggerParams.TriggerModel = CogAcqTriggerModelConstants.Auto;
                    else triggerParams.TriggerModel = CogAcqTriggerModelConstants.Manual;
                }
            }
            catch (CogException cogex)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, cogex.ToString());
            }
        }

        /// <summary>
        /// 3D Scanner Laser 제어
        /// </summary>
        /// <param name="bEnable"></param>
        public void LaserEnable(bool bEnable)
        {
            try
            {
                if (bEnable == true) cAcqFifoTool.Operator.FrameGrabber.OwnedGenTLAccess.SetFeature("RD_LaserPower", "200");
                else cAcqFifoTool.Operator.FrameGrabber.OwnedGenTLAccess.SetFeature("RD_LaserPower", "0");
            }
            catch (CogException cogex)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, cogex.ToString());
            }
        }

        private int AcqFifoTool_Ran_numacqs = 0;

        /// <summary>
        /// Acq image를 PatMaxTool 에 전달 Camera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AcqFifoTool_Ran(object sender, EventArgs e)
        {
            try
            {
                CogToolBlockClass cCogToolBlockClass = cCogToolBlockList.Find(x => x.uiToolBlockNo == uiToolBlockNo);
                cVisionToolBlockUI.DisplayClear();

                if (CMainLib.Ins.cVisionData.bRotateMode[uiCameraNo] == true)
                {
                    CogIPOneImageTool ImgTool = new CogIPOneImageTool();
                    CogIPOneImageFlipRotate cflip = new CogIPOneImageFlipRotate();

                    ICogIPOneImageOperatorParams ip = (ICogIPOneImageOperatorParams)cflip;
                    ImgTool.Operators.Add(ip);
                    if (CMainLib.Ins.cVisionData.eRotates[uiCameraNo] == eRotate._90Deg)
                        cflip.OperationInPixelSpace = CogIPOneImageFlipRotateOperationConstants.Rotate90Deg;
                    else if (CMainLib.Ins.cVisionData.eRotates[uiCameraNo] == eRotate._180Deg)
                        cflip.OperationInPixelSpace = CogIPOneImageFlipRotateOperationConstants.Rotate180Deg;
                    else if (CMainLib.Ins.cVisionData.eRotates[uiCameraNo] == eRotate._270Deg)
                        cflip.OperationInPixelSpace = CogIPOneImageFlipRotateOperationConstants.Rotate270Deg;
                    ImgTool.InputImage = cAcqFifoTool.OutputImage;
                    ImgTool.Run();

                    cVisionToolBlockUI.GetCogDisplay().Image = ImgTool.OutputImage as CogImage8Grey;
                    cCogToolBlockClass.cImageFileTool.InputImage = ImgTool.OutputImage as CogImage8Grey;
                    cCogToolBlockClass.cCogToolBlockEditV2.Subject.Inputs["InputImage"].Value = ImgTool.OutputImage as CogImage8Grey;
                }
                else
                {
                    cVisionToolBlockUI.GetCogDisplay().Image = cAcqFifoTool.OutputImage as CogImage8Grey;
                    cCogToolBlockClass.cImageFileTool.InputImage = cAcqFifoTool.OutputImage as CogImage8Grey;
                    if (cCogToolBlockClass.cCogToolBlockEditV2.Subject != null)
                        cCogToolBlockClass.cCogToolBlockEditV2.Subject.Inputs["InputImage"].Value = cAcqFifoTool.OutputImage as CogImage8Grey;
                }
            }
            catch (CogException cogex)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, cogex.ToString());
            }

            // Run the garbage collector to free unused images
            AcqFifoTool_Ran_numacqs++;
            if (AcqFifoTool_Ran_numacqs > 4)
            {
                GC.Collect();
                AcqFifoTool_Ran_numacqs = 0;
            }
        }

        /// <summary>
        /// Vision 촬영 명령
        /// </summary>
        /// <param name="uiToolBlockNo"></param>
        public bool VisionShoot(uint uiToolBlockNo)
        {
            if (Define.SIMULATION == true)
            {
                SimulationProcess();
                return true;
            }
            if (bRun == true)
            {
                // bRun 상태를 재확인하여 촬영을 진행
                bool bRetryOk = false;
                for (int i = 0; i < 100; i++)
                {
                    Thread.Sleep(10);
                    if (bRun == false)
                    {
                        bRetryOk = true;
                        break;
                    }
                }
                if (bRetryOk == false)
                {
                    NLogger.AddLog(eLogType.VisionShoot + (int)uiCameraNo, NLogger.eLogLevel.ERROR, "Camera Not Ready");
                    return false;
                }
            }

            try
            {
                bRun = true;
                this.uiToolBlockNo = uiToolBlockNo;
                CogToolBlockClass cCogToolBlockClass = cCogToolBlockList.Find(x => x.uiToolBlockNo == uiToolBlockNo);
                cAcqFifoTool.Run();

                cCogToolBlockClass.cCogToolBlockEditV2.Subject.Run();
                if (cCogToolBlockClass.cCogToolBlockEditV2.Subject.RunStatus.Exception != null &&
                    cCogToolBlockClass.cCogToolBlockEditV2.Subject.RunStatus.Exception.ToString().Contains("CogFixtureTool") == false &&
                    cCogToolBlockClass.cCogToolBlockEditV2.Subject.RunStatus.Exception.ToString().Contains("CogFindLineTool") == false &&
                    cCogToolBlockClass.cCogToolBlockEditV2.Subject.RunStatus.Exception.ToString().Contains("CogIntersectLineLineTool") == false)
                {
                    // 실패 결과 처리 (카메라 이미지 취득 실패만 넣으려면 어떻게?)
                    // CMainLib.Ins.VisionResultMessage(uiCameraNo, uiToolBlockNo, false, string.Empty);
                    NLogger.AddLog(eLogType.VisionShoot + (int)uiCameraNo, NLogger.eLogLevel.ERROR, cCogToolBlockClass.cCogToolBlockEditV2.Subject.RunStatus.Exception.Message);
                    bRun = false;
                }
                bRun = false;
            }
            catch (CogException cogex)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, cogex.ToString());
            }

            return true;
        }

        /// <summary>
        /// 시뮬레이션일 경우 처리
        /// </summary>
        public void SimulationProcess()
        {
            switch (uiCameraNo)
            {
                case (uint)eCAM.CAM0_PipeNeedlePickUp:
                    {
                        CVisionResultData cVisionResultData = CMainLib.Ins.cVar.GetVisionResultData(uiCameraNo, 0);
                        cVisionResultData.bGoodNg = true;
                        // 테스트를 위해 랜덤하게 값을 배정하여 설정한다.
                        byte[] data = new byte[1];
                        var rand = System.Security.Cryptography.RandomNumberGenerator.Create();
                        rand.GetBytes(data);
                        cVisionResultData.strBarcord = "SIMULATION";
                        cVisionResultData.bShootFinish = true;
                    }
                    break;

                case (uint)eCAM.CAM1_Pipe:
                    {
                        CVisionResultData cVisionResultData = CMainLib.Ins.cVar.GetVisionResultData(uiCameraNo, uiToolBlockNo);
                        cVisionResultData.dPipeMountX = 1.0;
                        // 테스트를 위해 랜덤하게 값을 배정하여 설정한다.
                        byte[] data = new byte[1];
                        var rand = System.Security.Cryptography.RandomNumberGenerator.Create();
                        rand.GetBytes(data);
                        if (data[0] % 2 == 0) cVisionResultData.bGoodNg = true;
                        else cVisionResultData.bGoodNg = false;
                        cVisionResultData.strBarcord = "SIMULATION";
                        cVisionResultData.bShootFinish = true;
                    }
                    break;

                case (uint)eCAM.CAM2_Needle:
                    {
                        CVisionResultData cVisionResultData = CMainLib.Ins.cVar.GetVisionResultData(uiCameraNo, uiToolBlockNo);
                        cVisionResultData.dPipeMountX = 1.0;
                        // 테스트를 위해 랜덤하게 값을 배정하여 설정한다.
                        byte[] data = new byte[1];
                        var rand = System.Security.Cryptography.RandomNumberGenerator.Create();
                        rand.GetBytes(data);
                        if (data[0] % 2 == 0) cVisionResultData.bGoodNg = true;
                        else cVisionResultData.bGoodNg = false;
                        cVisionResultData.strBarcord = "SIMULATION";
                        cVisionResultData.bShootFinish = true;
                    }
                    break;

                case (uint)eCAM.CAM3_PipeMount:
                    {
                        CVisionResultData cVisionResultData = CMainLib.Ins.cVar.GetVisionResultData(uiCameraNo, uiToolBlockNo);
                        cVisionResultData.bGoodNg = true;
                        cVisionResultData.strBarcord = "SIMULATION";
                        // 테스트를 위해 랜덤하게 값을 배정하여 설정한다.
                        byte[] data = new byte[1];
                        var rand = System.Security.Cryptography.RandomNumberGenerator.Create();
                        rand.GetBytes(data);
                        if (data[0] % 2 == 0) cVisionResultData.dPipeMountX = 1.0;
                        else cVisionResultData.dPipeMountX = 1.0;
                        cVisionResultData.bShootFinish = true;
                    }
                    break;

                case (uint)eCAM.CAM4_NeedleMount:
                    {
                        CVisionResultData cVisionResultData = CMainLib.Ins.cVar.GetVisionResultData(uiCameraNo, uiToolBlockNo);
                        cVisionResultData.bGoodNg = true;
                        cVisionResultData.dPipeMountX = 1.0;
                        cVisionResultData.dPipeMountY = 1.0;
                        cVisionResultData.bShootFinish = true;
                    }
                    break;

                case (uint)eCAM.CAM5_Dispenser:
                    {
                        CVisionResultData cVisionResultData = CMainLib.Ins.cVar.GetVisionResultData(uiCameraNo, uiToolBlockNo);
                        cVisionResultData.bGoodNg = true;
                        cVisionResultData.dPipeMountX = 1.0;
                        cVisionResultData.dPipeMountY = 1.0;
                        cVisionResultData.bShootFinish = true;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Vision 결과 값 클래스
    /// </summary>
    public class CVisionResult
    {
        public double dX;
        public double dY;
        public double dT;

        public CVisionResult(double dX = 0.0, double dY = 0.0, double dT = 0.0)
        {
            this.dX = dX;
            this.dY = dY;
            this.dT = dT;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", dX, dY, dT);
        }
    }
}