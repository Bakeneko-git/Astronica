import cv2
import socket
import mediapipe as mp
import time

def calc_land_pos(ln):
    x = pose_results.pose_landmarks.landmark[ln].x -0.5
    y = -(pose_results.pose_landmarks.landmark[ln].y -0.5)
    return x, y

HOST = '127.0.0.1'
PORT = 60000

# MediaPipe poses初期化
mp_pose = mp.solutions.pose
mp_drawing = mp.solutions.drawing_utils

pose = mp_pose.Pose(
    min_detection_confidence=0.5,   # 認識の最小信頼値
    min_tracking_confidence=0.5,    # トラッキングの最小信頼値
    model_complexity = 0    #軽量モード（デフォルトは1）
)


if __name__ == "__main__": 
    #通信クライアント作成
    client = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    #カメラの設定　デバイスIDは0
    cap = cv2.VideoCapture(0, cv2.CAP_MSMF)
    #cap.set(cv2.CAP_PROP_FOURCC, cv2.VideoWriter_fourcc('M','J','P','G'))# 圧縮フォーマット
    cap.set(cv2.CAP_PROP_FOURCC, cv2.VideoWriter_fourcc('H','2','6','4'))# 圧縮フォーマット
    cap.set(cv2.CAP_PROP_BUFFERSIZE, 1)     #バッファサイズ
    cap.set(cv2.CAP_PROP_FPS, 60)           # カメラFPSを60FPSに設定
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 32) # カメラ画像の横幅を1280に設定
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 18) # カメラ画像の縦幅を720に設定

    #入力画像の状態確認
    print("fps:",cap.get(cv2.CAP_PROP_FPS))
    print("Format:",cap.get(cv2.CAP_PROP_FOURCC))
    print("Width:",cap.get(cv2.CAP_PROP_FRAME_WIDTH))
    print("Height:",cap.get(cv2.CAP_PROP_FRAME_HEIGHT))

    #繰り返しのためのwhile文
    while cap.isOpened():

        #カメラからの画像取得
        ret, frame = cap.read()

        # MediaPipeで扱う画像は、OpenCVのBGRの並びではなくRGBのため変換
        rgb_image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

        # 画像をリードオンリーにしてposes検出処理実施
        
        rgb_image.flags.writeable = False
        pose_results = pose.process(rgb_image)
        rgb_image.flags.writeable = True

        # 有効なランドマークが検出された場合、ランドマークを描画
        if pose_results.pose_landmarks:
            #ランドマークの表示
            #mp_drawing.draw_landmarks(frame, pose_results.pose_landmarks,
            #                            mp_pose.POSE_CONNECTIONS)
            wants_landmarks = [
                0,  #nose
                19, #left_hand
                20, #right_hand
                23, #left_hip
                24, #right_hip
                11, #left_shoulder
                12, #right_shoulder
            ]

            #送信データ作成
            result = []
            for i in wants_landmarks:
                d = calc_land_pos(i)
                result += [str(e) for e in d]
            result = ",".join(result)

            #データ送信
            client.sendto(result.encode('utf-8'),(HOST,PORT))

        #カメラの画像の出力
        #cv2.imshow('camera' , frame)

        #繰り返し分から抜けるためのif文
        #k = cv2.waitKey(1)
        #if k == ord("q"):
            #break
                

    #メモリを解放して終了するためのコマンド
    cap.release()
    cv2.destroyAllWindows()