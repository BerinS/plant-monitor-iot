import { Component, computed } from '@angular/core';
import { LucideAngularModule, Droplets, Pen, Trash2 } from 'lucide-angular';


@Component({
  selector: 'app-plant-card-my-plants',
  imports: [LucideAngularModule],
  template: `
    <div class="plant-card">

      
      <div class="header">        
        <div>
          <h3>Bedside Peace Lilly</h3>
        </div>
        <div class="corner-icon">      
          <span class="pen"><lucide-icon [img]="PenIcon" size="20"></lucide-icon> </span>  
          <span class="trash"><lucide-icon [img]="TrashIcon" size="20"></lucide-icon> </span>
        </div>        
      </div>   
      
      <div class="wrapper">    
            
        <div class="item1">          
          <div class="description">
            <p>Peace lilly on the bedside table, baby blue vase. Gifted to me by a friend from work.</p>
          </div>   
          <div class="footer">
            <div class="plant-info"><b>GROUP:</b> Bedroom</div>
            <div class="plant-info"><b>CREATED AT:</b> 14:52 | 17/01/2026</div>        
            <div class="plant-info"><b>SENSOR:</b> AA:BB:CC:DD:EE:FF</div>        
          </div>
        </div>

        <div class="item2 stats">
          <div class="moisture-badge">
            <lucide-icon [img]="DropIcon" size="37"></lucide-icon>
            <span class="moisture-num">45%</span>
          </div>        
        </div>

      </div>

      

    </div>
    `,
  styleUrl: './plant-card-my-plants.component.scss',
})
export class PlantCardMyPlants {
  readonly DropIcon = Droplets;
  readonly PenIcon = Pen;
  readonly TrashIcon = Trash2;

  //isLow = computed(() => this.plant().currentMoisture < 30);
}
