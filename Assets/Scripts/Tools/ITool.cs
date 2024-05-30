using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITool
{
    void SetManager(ToolsManager manager);

    Tool GetToolType();

}
