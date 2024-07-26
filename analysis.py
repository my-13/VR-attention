import seaborn as sns
import numpy as numpy
import matplotlib.pyplot as plt
import os

data_path = "./data/"

def graphMeanRTData(meanData):
    sns.set_theme(style="ticks")

    sns.set()
    plt.figure()

    f, ax = sns.boxplot(data=titanic, x = "Category", y="Mean RT", hue="alive")

    ax.set_title("Mean Response Time")
    ax.grid(color="#cccccc")

    plt.show()

def grabData():

    for file in os.listdir(data_path):
        filename = os.fsdecode(file)
        filename_array = filename.split("_")

        if(filename_array[0] == "main"):
            #Sorting all Main Data Array
            if (filename_array[2] in ["16"]):
                #If it has a distractor
                pass
            if (filename_array[2] in ["14"]):
                #If it doesn't have a distractor
                pass

    
    # W Distractor, W/O Distractor
    mean_rt_data = []
