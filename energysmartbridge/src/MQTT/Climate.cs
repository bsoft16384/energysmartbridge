﻿using System.Collections.Generic;

internal class Climate : Device
{
    public string action_template { get; set; }
    public string action_topic { get; set; }
    public string current_temperature_topic { get; set; }

    public string temperature_state_topic { get; set; }
    public string temperature_command_topic { get; set; }

    public string max_temp { get; set; }

    public string mode_state_topic { get; set; }
    public string mode_command_topic { get; set; }
    public List<string> modes { get; set; }
}