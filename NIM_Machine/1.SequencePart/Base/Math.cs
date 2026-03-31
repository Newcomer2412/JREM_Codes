using System;

namespace MachineControlBase
{
    public static class CMath
    {
        /// <summary>
        /// 노즐 T의 회전 오차를 보정하기 위한 타원의 계산식
        /// </summary>
        /// <param name="iNozzle_W">타원의 가로 길이</param>
        /// <param name="iNozzle_H">타원의 세로 길이</param>
        /// <param name="iNozzleTiltDegree">타원의 기울어진 기울기</param>
        /// <param name="dTheta">회전 각도</param>
        /// <param name="iOutX">리턴 값</param>
        /// <param name="iOutY">리턴 값</param>
        public static void GetNozzleDegreeOffset(int iNozzle_W, int iNozzle_H, int iNozzleTiltDegree,
                                                    double dTheta, ref int iOutX, ref int iOutY)
        {
            double dW = (double)iNozzle_W;              // 타원의 쟝축 거리 X / 2
            double dH = (double)iNozzle_H;              // 타원의 단축 거리 Y / 2
            double dRadian = dTheta * Math.PI / 180;  // 노즐의 각도 위치
            double dTiltRadian = (double)iNozzleTiltDegree * Math.PI / 180; // 타원의 기울기 값

            // 타원의 좌표 공식 사용 (Vision 회전 각도 기준과 타원 방정식 기준 통일 필요. 타원 방정식 좌로 회전 기준)
            // iOutX의 경우 T축 회전 방향의 +방향이 반시계 방향일 경우 * -1을 하지 않는다.
            iOutX = (int)((dW * Math.Cos(dRadian) * Math.Cos(dTiltRadian)) - (dH * Math.Sin(dRadian) * Math.Sin(dTiltRadian))) * -1;
            iOutY = (int)((dW * Math.Cos(dRadian) * Math.Sin(dTiltRadian)) + (dH * Math.Sin(dRadian) * Math.Cos(dTiltRadian)));
        }

        /// <summary>
        /// 회전에 대한 보정된 위치 값 계산 (회전 변환 공식)
        /// </summary>
        /// <param name="dVisionOffsetX">Vision에서의 센터 X 오차 값</param>
        /// <param name="dVisionOffsetY">Vision에서의 센터 Y 오차 값</param>
        /// <param name="dTheta">변경될 각도 값</param>
        /// <param name="dPosX">현재 X위치 값</param>
        /// <param name="dPosY">현재 Y위치 값</param>
        /// <param name="iOutX">각도 변경 후 X 변화량</param>
        /// <param name="iOutY">각도 변경 후 Y 변화량</param>
        public static void GetRotatePos(double dVisionOffsetX, double dVisionOffsetY,
                                          double dTheta, double dPosX, double dPosY,
                                          ref int iOutX, ref int iOutY)
        {
            double dRadian = dTheta * Math.PI / 180; // 노즐의 각도 위치

            double dCalX = dPosX - dVisionOffsetX;
            double dCalY = dPosY - dVisionOffsetY;

            iOutX = (int)(((Math.Cos(dRadian) * dCalX) - (Math.Sin(dRadian) * dCalY)) * 1000); // 펄스 단위기 때문에 1000을 곱해줌
            iOutY = (int)(((Math.Sin(dRadian) * dCalX) + (Math.Cos(dRadian) * dCalY)) * 1000);
        }

        /// <summary>
        /// TR Robot Hand 각도 값 계산
        /// </summary>
        /// <param name="dPos">이송 하려는 위치 값</param>
        /// <returns>이송 위치 값으로 가려면 이송해야하는 각도 값</returns>
        public static double GetTRRobotHandTheta(double dPos)
        {
            double dOnePointToTwoPointDistance = 165;
            double dRadian = Math.Asin(dPos / (2 * dOnePointToTwoPointDistance));
            double dTheta = Math.Round(dRadian * (180 / Math.PI), 3);
            return dTheta;
        }

        /// <summary>
        /// TR Robot Hand 위치 값 계산
        /// </summary>
        /// <param name="dTheta">각도 값</param>
        /// <returns>이송한 위치 값</returns>
        public static double GetTRRobotHandPos(double dTheta)
        {
            double dOnePointToTwoPointDistance = 165;
            double dRadian = dTheta * Math.PI / 180; // 각도 -> 라디안
            double dPos = Math.Round(2 * dOnePointToTwoPointDistance * Math.Sin(dRadian), 3);
            return dPos;
        }

        /// <summary>
        /// 니들삽입기 Flip Z축 각도 계산
        /// </summary>
        /// <param name="dVertical">높이 값</param>
        /// <returns>이송할 각도 값</returns>
        public static double GetFlipZ_AnglePos(double dVertical)
        {
            double dr_Distance = 15;
            double dAsin = Math.Asin(dVertical / dr_Distance);
            double dAngle = Math.Round(dAsin / Math.PI * 180, 3); // 라디안 -> 각도
            return dAngle;
        }

        /// <summary>
        /// 니들 삽입기 홀더 회전 변환
        /// </summary>
        /// <param name="dTheta">변경될 각도 값</param>
        /// <param name="dPosX">현재 X위치 값</param>
        /// <param name="dPosY">현재 Y위치 값</param>
        /// <param name="iOutX">각도 변경 후 X 변화량</param>
        /// <param name="iOutY">각도 변경 후 Y 변화량</param>
        public static void GetRotateTransform(double dTheta, double dPosX, double dPosY,
                                              ref double iOutX, ref double iOutY)
        {
            //double dRadian = dTheta * Math.PI / 180; // 노즐의 각도 위치

            iOutX = (Math.Cos(dTheta) * dPosX) + (Math.Sin(dTheta) * dPosY);
            iOutY = (Math.Sin(dTheta) * dPosX) + (Math.Cos(dTheta) * dPosY);
        }
    }
}