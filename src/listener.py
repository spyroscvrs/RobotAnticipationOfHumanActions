import rospy
from std_msgs.msg import String


import sys
import copy
import moveit_commander
import moveit_msgs.msg
import geometry_msgs.msg
from math import pi
from moveit_commander.conversions import pose_to_list


moveit_commander.roscpp_initialize(sys.argv)
#rospy.init_node('move_group_python_interface_tutorial',anonymous=True)
robot = moveit_commander.RobotCommander()
scene = moveit_commander.PlanningSceneInterface()

group_name = "manipulator"
group = moveit_commander.MoveGroupCommander(group_name)

display_trajectory_publisher = rospy.Publisher('/move_group/display_planned_path',moveit_msgs.msg.DisplayTrajectory,queue_size=20)


joint_goal=group.get_current_joint_values()
##


buff=[]

def average(lst):
 return float(sum(lst))/len(lst)



#SO CALLBACK JUST GETS THE NEW MESSAGE
def callback(data): #when new data is received callback is invoked with data as the first argument
    rospy.loginfo(rospy.get_caller_id() + ':	%s', data.data)
    buff.append(int(data.data))
    rospy.loginfo(buff)
    if len(buff)==5:
        avg=average(buff)
        rospy.loginfo(avg)
        del buff[:]

        if avg<-0.5: #left
            joint_goal[0]=-0.298946
            joint_goal[1]=-0.231593
            joint_goal[2]=0.1570933
            joint_goal[3]=0.0744128
            joint_goal[4]=-0.29881
            joint_goal[5]=-3.63886e-05
        elif avg>0.5: #right
            joint_goal[0]=0.22236
            joint_goal[1]=-0.117996
            joint_goal[2]=0.0309145
            joint_goal[3]=0.086955
            joint_goal[4]=0.2223
            joint_goal[5]=-3.345555e-05
        else: #unknown
            joint_goal[0]=-0.0207869
            joint_goal[1]=-0.501842
            joint_goal[2]=0.5019
            joint_goal[3]=0
            joint_goal[4]=-0.020777
            joint_goal[5]=0.000502
        group.go(joint_goal,wait=True)
    # Calling `stop()` ensures that there is no residual movement
        group.stop()            


def listener():
    rospy.init_node('listener', anonymous=True) #node name
    rospy.Subscriber('chatter', String, callback) #topic name
    # spin() simply keeps python from exiting until this node is stopped

  
    rospy.spin()

if __name__ == '__main__':
    listener()
