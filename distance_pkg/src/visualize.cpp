#include <ros/ros.h>
#include <custom_msgs/dis_point.h>
#include <custom_msgs/dis_points.h>
#include <visualization_msgs/Marker.h>
#include <visualization_msgs/MarkerArray.h>
#include <opencv2/opencv.hpp>
using namespace std;
using namespace cv;

Mat left_intrinsic = Mat_<float>(3, 3);
Mat left_extrinsic = Mat_<float>(3, 4);
Mat left_distCoeffs = Mat_<float>(5, 1);

Mat right_intrinsic = Mat_<float>(3, 3);
Mat right_extrinsic = Mat_<float>(3, 4);
Mat right_distCoeffs = Mat_<float>(5, 1);
vector<float> right_e;
ros::Publisher left_markerArrayPub;
ros::Publisher right_markerArrayPub;//script as custom_msgs::markerArray left_markerArrayPub after changes

Mat mat2xyz(Mat &input_dis, Mat &intrinsic, Mat &extrinsic) {
    cv::Mat intrinsic_inv = intrinsic.inv();
    cv::Mat extrinsic_pinv = extrinsic.pinv();
    cv::Mat input_dis_2 = cv::Mat::zeros(4, input_dis.cols, CV_32F);
    for (int i = 0; i < input_dis.cols; i++) {
        input_dis_2.at<float>(0,i) = round(res.at<float>(0, i) * res.at<float>(2, i));
        input_dis_2.at<float>(1,i) = round(res.at<float>(1, i) * res.at<float>(2, i));
        input_dis_2.at<float>(2,i) = round(res.at<float>(2, i));
    }
    Mat res = extrinsic_pinv * intrinsic_inv * input_dis_2;
    return res;
}

Mat DisPointMsg2mat(const std::vector<dis_point.msg>& points) {
    Mat ret = cv::Mat::zeros(3, points.size(), CV_32F);
    for (int k = 0; k < points.size(); k++) {
        ret.at<float>(0, k) = points[k].x;
        ret.at<float>(1, k) = points[k].y;
        ret.at<float>(2, k) = points[k].dist;
    }
    return ret;
}
void left_disPointCallback(custom_msgs::dis_points::ConstPtr& msg)
{
    visualization_msgs::MarkerArray markerArray1;
    visualization_msgs::MarkerArray markerArray2;
    markerArray1.markers.resize(msg->data.size());
    markerArray2.markers.resize(msg->data.size());
    for (size_t i = 0; i < msg->data.size(); ++i) {
        const auto& disPoint = msg->data[i];
        Mat left_pointMat = Mat::zeros(4, data.size(), CV_32FC1);
        Mat left_coordMat = Mat::zeros(4, data.size(), CV_32FC1);
        Mat left_pointMat = DisPointMsg2mat(disPoint); 
        Mat left_coordMat = mat2xyz(left_pointMat, left_intrinsic, left_extrinsic);

        // Create a marker for each dis_point
        visualization_msgs::Marker& marker = markerArray1.markers[i];
        marker.header = msg->header;
        marker.ns = "dis_points_markers";
        marker.id = disPoint.id;
        marker.type = visualization_msgs::Marker::TEXT_VIEW_FACING;
        marker.action = visualization_msgs::Marker::ADD;
        marker.pose.position.x = left_coordMat.at<float>(0,i);
        marker.pose.position.y = left_coordMat.at<float>(1,i);
        marker.pose.position.z = left_coordMat.at<float>(2,i);
        marker.scale.z = 0.1;
        marker.color.a = 1.0;
        marker.color.r = 1.0;
        marker.color.g = 1.0;
        marker.color.b = 1.0;
        marker.text = disPoint.text;
    }
 
    for (size_t i = 0; i < msg->data.size(); ++i) {
        const auto& disPoint = msg->data[i];
        Mat left_pointMat = Mat::zeros(4, data.size(), CV_32FC1);
        Mat left_coordMat = Mat::zeros(4, data.size(), CV_32FC1);
        Mat left_pointMat = DisPointMsg2mat(disPoint); 
        Mat left_coordMat = mat2xyz(left_pointMat, left_intrinsic, left_extrinsic);
        visualization_msgs::Marker& marker = markerArray2.markers[i];
        marker.header = msg->header;
        marker.ns = "dis_point_markers";
        marker.type = visualization_msgs::Marker::SPHERE;
        marker.action = visualization_msgs::Marker::ADD;
        marker.pose.orientation.w = 1.0;
        marker.scale.x = 0.1;
        marker.scale.y = 0.1;
        marker.scale.z = 0.1;
        marker.color.a = 1.0;
        marker.pose.position.x = round(pointMat.at<float>(0, i));
        marker.pose.position.y = round(pointMat.at<float>(1, i));
        marker.pose.position.z = round(pointMat.at<float>(2, i));

        if (disPoint.color == 0) {
            marker.color.r = 0.0;
            marker.color.g = 0.0;
            marker.color.b = 1.0; 
        } else {
            marker.color.r = 1.0;
            marker.color.g = 0.0;
            marker.color.b = 0.0; 
        }
    }
    left_markerArrayPub.publish(markerArray1);
    left_markerArrayPub.publish(markerArray2);
}

void right_disPointCallback(custom_msgs::dis_points::ConstPtr& msg)
{
    visualization_msgs::MarkerArray markerArray1;
    visualization_msgs::MarkerArray markerArray2;
    markerArray1.markers.resize(msg->data.size());
    markerArray2.markers.resize(msg->data.size());
    for (size_t i = 0; i < msg->data.size(); ++i) {
        const auto& disPoint = msg->data[i];
        Mat right_pointMat = Mat::zeros(4, data.size(), CV_32FC1);
        Mat right_coordMat = Mat::zeros(4, data.size(), CV_32FC1);
        Mat right_pointMat = DisPointMsg2mat(disPoint); 
        Mat righy_coordMat = mat2xyz(left_pointMat, left_intrinsic, left_extrinsic);

        // Create a marker for each dis_point
        visualization_msgs::Marker& marker = markerArray1.markers[i];
        marker.header = msg->header;
        marker.ns = "dis_points_markers";
        marker.id = disPoint.id;
        marker.type = visualization_msgs::Marker::TEXT_VIEW_FACING;
        marker.action = visualization_msgs::Marker::ADD;
        marker.pose.position.x = left_coordMat.at<float>(0,i);
        marker.pose.position.y = left_coordMat.at<float>(1,i);
        marker.pose.position.z = left_coordMat.at<float>(2,i);
        marker.scale.z = 0.1;
        marker.color.a = 1.0;
        marker.color.r = 1.0;
        marker.color.g = 1.0;
        marker.color.b = 1.0;
        marker.text = disPoint.text;
    }
 
    for (size_t i = 0; i < msg->data.size(); ++i) {
        const auto& disPoint = msg->data[i];
        Mat left_pointMat = Mat::zeros(4, data.size(), CV_32FC1);
        Mat left_coordMat = Mat::zeros(4, data.size(), CV_32FC1);
        Mat left_pointMat = DisPointMsg2mat(disPoint); 
        Mat left_coordMat = mat2xyz(left_pointMat, left_intrinsic, left_extrinsic);
        visualization_msgs::Marker& marker = markerArray2.markers[i];
        marker.header = msg->header;
        marker.ns = "dis_point_markers";
        marker.type = visualization_msgs::Marker::SPHERE;
        marker.action = visualization_msgs::Marker::ADD;
        marker.pose.orientation.w = 1.0;
        marker.scale.x = 0.1;
        marker.scale.y = 0.1;
        marker.scale.z = 0.1;
        marker.color.a = 1.0;
        marker.pose.position.x = round(pointMat.at<float>(0, i));
        marker.pose.position.y = round(pointMat.at<float>(1, i));
        marker.pose.position.z = round(pointMat.at<float>(2, i));

        if (disPoint.color == 0) {
            marker.color.r = 0.0;
            marker.color.g = 0.0;
            marker.color.b = 1.0; 
        } else {
            marker.color.r = 1.0;
            marker.color.g = 0.0;
            marker.color.b = 0.0; 
        }
    }
    right_markerArrayPub.publish(markerArray1);
    right_markerArrayPub.publish(markerArray2);
}


int main(int argc, char** argv)
{
    ros::init(argc, argv, "visualize_node");
    ros::NodeHandle nh;
    ros::param::get("/left_cam/intrinsic_matrix", left_i);
    for(int i = 0; i < 9; i++) left_intrinsic.at<float>(i / 3, i % 3) = left_i[i];
    ros::param::get("/left_cam/extrinsic_matrix", left_e);
    for(int i = 0; i < 12; i++) left_extrinsic.at<float>(i / 4, i % 4) = left_e[i];
    ros::param::get("/left_cam/distortion_coefficients", left_d);
    for(int i = 0; i < 5; i++) left_distCoeffs.at<float>(i, 0) = left_d[i];

    // cout << left_intrinsic << "\n" << left_extrinsic << "\n" << left_distCoeffs << endl;

    ros::param::get("/right_cam/intrinsic_matrix", right_i);
    for(int i = 0; i < 9; i++) right_intrinsic.at<float>(i / 3, i % 3) = right_i[i];
    ros::param::get("/right_cam/extrinsic_matrix", right_e);
    for(int i = 0; i < 12; i++) right_extrinsic.at<float>(i / 4, i % 4) = right_e[i];
    ros::param::get("/right_cam/distortion_coefficients", right_d);
    for(int i = 0; i < 5; i++) right_distCoeffs.at<float>(i, 0) = right_d[i];

    
    left_markerArrayPub = nh.advertise<visualization_msgs::MarkerArray>("/left/marker_array_topic", 1);
    right_markerArrayPub = nh.advertise<visualization_msgs::MarkerArray>("/right/marker_array_topic", 1);

    ros::Subscriber disPointSub = nh.subscribe("/left/dis_point", 1, left_disPointCallback);
    ros::Subscriber disPointsSub = nh.subscribe("/left/dis_points", 1, right_disPointCallback);

    ros::spin();

    return 0;
}
